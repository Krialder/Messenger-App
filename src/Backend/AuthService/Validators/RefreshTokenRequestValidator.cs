using FluentValidation;
using MessengerContracts.DTOs;

namespace AuthService.Validators
{
    /// <summary>
    /// Validator for refresh token requests
    /// </summary>
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
}
