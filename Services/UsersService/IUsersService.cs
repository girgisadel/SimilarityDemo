namespace SimilarityDemo.Services.UsersService;

public interface IUsersService
{
    Task<bool> IsNameAvailableAsync(string userName);

    Task<bool> IsNameInUseAsync(string userName);

    Task<double> GetNameScoreAsync(string userName);

    Task<List<string>> GetNameSuggestionsAsync(string userName, string? firstName = null,
        string? lastName = null, int count = 3);
}
