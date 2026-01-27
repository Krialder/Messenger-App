using FluentValidation;
using MessengerContracts.DTOs;

namespace UserService.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Display name must not exceed 100 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Bio))
            .WithMessage("Bio must not exceed 500 characters");

        RuleFor(x => x.AvatarUrl)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl))
            .WithMessage("Avatar URL must be a valid URL");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class AddContactRequestValidator : AbstractValidator<AddContactRequest>
{
    public AddContactRequestValidator()
    {
        RuleFor(x => x.ContactUserId)
            .NotEmpty()
            .WithMessage("Contact user ID is required");

        RuleFor(x => x.Nickname)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Nickname))
            .WithMessage("Nickname must not exceed 100 characters");
    }
}

public class SearchQueryValidator : AbstractValidator<string>
{
    public SearchQueryValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Search query is required")
            .MinimumLength(2)
            .WithMessage("Search query must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Search query must not exceed 100 characters");
    }
}

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required for account deletion");
    }
}
