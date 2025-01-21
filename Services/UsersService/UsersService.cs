using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SimilarityDemo.Data.Contexts;
using SimilarityDemo.Data.Entities;
using SimilarityDemo.Identity;
using System.Text.RegularExpressions;

namespace SimilarityDemo.Services.UsersService;

public class UsersService(InternalUserManager<User> userManager, 
    IdentityDatabase identityDatabase, 
    IOptions<IdentityOptions> optionsAccessor) : IUsersService
{
    public async Task<double> GetNameScoreAsync(string userName)
    {
        if (await IsNameInUseAsync(userName))
        {
            return 0.0;
        }

        userName = userManager.NormalizeName(userName);

        var usersCountQuery = @"
                SELECT Count(NormalizedUserName) AS Count
                FROM AspNetUsers 
                WHERE LEN(NormalizedUserName) = LEN({0}) AND DIFFERENCE(NormalizedUserName, {0}) >= 1";

        var usersCount = await identityDatabase.Database
          .SqlQueryRaw<CountDto>(usersCountQuery, userName)
          .SingleOrDefaultAsync();

        if (usersCount is not null && usersCount.Count != 0)
        {
            var similarityCountQuery = @"
                SELECT Count(NormalizedUserName) AS Count
                FROM AspNetUsers 
                WHERE LEN(NormalizedUserName) = LEN({0}) AND DIFFERENCE(NormalizedUserName, {0}) >= 3";

            var similarityCount = await identityDatabase.Database
                .SqlQueryRaw<CountDto>(similarityCountQuery, userName)
                .SingleOrDefaultAsync();

            return Math.Round(1 - (Math.Log((similarityCount?.Count ?? 0) + 1) / Math.Log(usersCount.Count + 1)), 2);
        }

        return 0.0;
    }

    public async Task<List<string>> GetNameSuggestionsAsync(string userName, 
        string? firstName = null, 
        string? lastName = null, 
        int count = 3)
    {
        var names = GetNames(userName);
        count = Math.Clamp(count, 3, 5);
        var suggestions = new List<string>();
        var random = new Random();
        var length = names.Length;
        const int attemptsLimit = 100;
        for (int i = 0; i < count; i++)
        {
            int attempts = 0;
            SuggestionDto? suggestion;

            do
            {
                if (attempts >= attemptsLimit)
                {
                    suggestion = null;
                    break;
                }

                attempts++;

                var innerFirstName = firstName ?? names[random.Next(0, length)];
                var innerlLastName = lastName ?? names[random.Next(0, length)];

                suggestion = new Faker<SuggestionDto>()
                    .RuleFor(n => n.UserName, f => f.Internet.UserName(innerFirstName, innerlLastName))
                    .Generate();

            } while (!await IsValidSuggestedUserName(suggestion.UserName, userName) || suggestions.Contains(suggestion.UserName));

            if (suggestion is not null && !string.IsNullOrEmpty(suggestion.UserName))
            {
                suggestions.Add(suggestion.UserName);
            }
        }
        return suggestions.Count != 0 ? suggestions : Enumerable.Empty<string>().ToList();
    }

    private string[] GetNames(string userName)
    {
        var options = optionsAccessor.Value;
        var specialChars = options.User.AllowedUserNameCharacters
            .Where(c => !char.IsLetterOrDigit(c))
            .ToArray();
        var pattern = $"[{Regex.Escape(new string(specialChars))}]";

        return Regex
            .Split(userName, pattern, RegexOptions.None)
            .Where(name => !string.IsNullOrEmpty(name))
        .ToArray();
    }

    private async Task<bool> IsValidSuggestedUserName(string suggestedUserName, string userName)
    {
        var options = optionsAccessor.Value;

        var isValidCharacters = suggestedUserName.All(c => options.User.AllowedUserNameCharacters.Contains(c));
        var isNotOriginalUserName = !suggestedUserName.Equals(userName, StringComparison.OrdinalIgnoreCase);
        var isAvailable = await IsNameAvailableAsync(suggestedUserName);

        return isAvailable && isValidCharacters && isNotOriginalUserName;
    }

    public async Task<bool> IsNameAvailableAsync(string userName)
    {
        return await userManager.IsNameInUseAsync(userName) is false;
    }

    public Task<bool> IsNameInUseAsync(string userName)
    {
        return userManager.IsNameInUseAsync(userName);
    }
}
