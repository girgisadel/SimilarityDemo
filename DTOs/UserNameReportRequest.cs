using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace SimilarityDemo.DTOs;

public record UserNameReportRequest(string UserName, string? FirstName, string? LastName);

public class UserNameReportRequestValidator : AbstractValidator<UserNameReportRequest>
{
    public UserNameReportRequestValidator(IOptions<IdentityOptions> options)
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

        RuleFor(request => request.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("First name cannot be empty.")
            .When(r => !string.IsNullOrEmpty(r.FirstName))

            .MinimumLength(3)
            .WithMessage("First name be at least 3 characters long.")
            .When(r => !string.IsNullOrEmpty(r.FirstName))

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
            .WithMessage($"First name contains invalid characters.")
            .When(r => !string.IsNullOrEmpty(r.FirstName));

        RuleFor(request => request.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Last name cannot be empty.")
            .When(r => !string.IsNullOrEmpty(r.LastName))

            .MinimumLength(3)
            .WithMessage("Last name must be at least 3 characters long.")
            .When(r => !string.IsNullOrEmpty(r.LastName))

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
            .WithMessage($"Last name contains invalid characters.")
            .When(r => !string.IsNullOrEmpty(r.LastName));
    }
}