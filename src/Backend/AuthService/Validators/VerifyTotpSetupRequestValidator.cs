using FluentValidation;
using MessengerContracts.DTOs;

namespace AuthService.Validators;

/// <summary>
/// Validator for TOTP setup verification requests
/// </summary>
public class VerifyTotpSetupRequestValidator : AbstractValidator<VerifyTotpSetupRequest>
{
    public VerifyTotpSetupRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("TOTP code is required")
            .Length(6)
            .WithMessage("TOTP code must be exactly 6 digits")
            .Matches(@"^\d{6}$")
            .WithMessage("TOTP code must contain only digits");
    }
}
