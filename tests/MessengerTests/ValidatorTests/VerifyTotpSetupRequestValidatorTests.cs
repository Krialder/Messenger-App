using AuthService.Validators;
using FluentValidation.TestHelper;
using MessengerContracts.DTOs;
using Xunit;

namespace MessengerTests.ValidatorTests;

public class VerifyTotpSetupRequestValidatorTests
{
    private readonly VerifyTotpSetupRequestValidator _validator;

    public VerifyTotpSetupRequestValidatorTests()
    {
        _validator = new VerifyTotpSetupRequestValidator();
    }

    [Fact]
    public void Code_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new VerifyTotpSetupRequest("123456");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Code_WhenEmpty_ShouldHaveValidationError(string code)
    {
        // Arrange
        var request = new VerifyTotpSetupRequest(code);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("TOTP code is required");
    }

    [Theory]
    [InlineData("12345")]    // Too short
    [InlineData("1234567")]  // Too long
    public void Code_WhenInvalidLength_ShouldHaveValidationError(string code)
    {
        // Arrange
        var request = new VerifyTotpSetupRequest(code);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("TOTP code must be exactly 6 digits");
    }

    [Theory]
    [InlineData("12345a")]   // Contains letter
    [InlineData("123 56")]   // Contains space
    [InlineData("123-56")]   // Contains dash
    [InlineData("abcdef")]   // All letters
    public void Code_WhenNotDigitsOnly_ShouldHaveValidationError(string code)
    {
        // Arrange
        var request = new VerifyTotpSetupRequest(code);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("TOTP code must contain only digits");
    }

    [Theory]
    [InlineData("000000")]
    [InlineData("999999")]
    [InlineData("123456")]
    [InlineData("654321")]
    public void Code_WhenValidSixDigits_ShouldNotHaveValidationError(string code)
    {
        // Arrange
        var request = new VerifyTotpSetupRequest(code);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
