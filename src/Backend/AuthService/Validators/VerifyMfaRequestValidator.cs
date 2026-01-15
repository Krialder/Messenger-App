using FluentValidation;
using MessengerContracts.DTOs;

namespace AuthService.Validators
{
    /// <summary>
    /// Validator for MFA verification requests
    /// </summary>
    public class VerifyMfaRequestValidator : AbstractValidator<VerifyMfaRequest>
    {
        public VerifyMfaRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.MfaCode)
                .NotEmpty().WithMessage("MFA code is required")
                .Length(6).WithMessage("MFA code must be 6 digits")
                .Matches("^[0-9]{6}$").WithMessage("MFA code must be 6 digits");
        }
    }
}
