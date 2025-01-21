using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimilarityDemo.Data.Entities;

namespace SimilarityDemo.Services.SeedingService;

public class SeedingService(UserManager<User> userManager,
    ILogger<SeedingService> logger) : ISeedingService
{
    public async Task SeedAsync()
    {
        var size = 10000;
        var count = size;

        var userFaker = new Faker<UserDto>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Password, f => f.Internet.Password(prefix: "Pass@123"))
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());

        var fakeUsers = userFaker.Generate(size);

        if (!await userManager.Users.AnyAsync())
        {
            foreach (var fakeUser in fakeUsers)
            {
                var user = new User();

                await userManager.SetUserNameAsync(user, fakeUser.UserName);
                await userManager.SetEmailAsync(user, fakeUser.Email);
                await userManager.SetPhoneNumberAsync(user, fakeUser.PhoneNumber);

                user.FirstName = fakeUser.FirstName;
                user.LastName = fakeUser.LastName;
                user.EmailConfirmed = true;
                user.PhoneNumberConfirmed = true;

                try
                {
                    var result = await userManager.CreateAsync(user, fakeUser.Password);
                    if (!result.Succeeded)
                    {
                        count--;
                    }
                }
                catch (Exception ex)
                {
                    count--;
                    logger.LogError(ex, "An exception has occurred while seeding `{UserName}`.", user.UserName);
                    continue;
                }
            }

            logger.LogInformation("{count} users successfully seeded out of {size}", count, size);
        }
    }
}
