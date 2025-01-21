using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace SimilarityDemo.DTOs;

public record UserNameRequest(string UserName);

public class UserNameRequestValidator : AbstractValidator<UserNameRequest>
{
    public UserNameRequestValidator(IOptions<IdentityOptions> options)
    {
        var identityOptions = options.Value;

        RuleFor(request => request.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Username is required.")

            .NotEmpty()
            .WithMessage("Username cannot be empty.");

        RuleFor(request => request.UserName)
            .Cascade(CascadeMode.Continue)

            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.")

            .Must(chars =>
            {
                var containsInvalidChar = false;
                foreach (var c in chars)
                {
                    if (!identityOptions.User.AllowedUserNameCharacters.Contains(c))
                    {
                        containsInvalidChar = true;
                        break;
                    }
                }
                return !containsInvalidChar;
            })
            .WithMessage($"Username contains invalid characters.");
    }
}