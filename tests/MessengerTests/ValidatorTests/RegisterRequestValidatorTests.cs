using AuthService.Validators;
using FluentValidation.TestHelper;
using MessengerContracts.DTOs;
using Xunit;

namespace MessengerTests.ValidatorTests;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Theory]
    [InlineData("ValidUser123", "valid@example.com", "StrongPass123!")]
    [InlineData("User", "test@test.com", "Pass123!@#")]
    public void Register_ValidRequest_ShouldNotHaveValidationErrors(string username, string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(username, email, password);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "valid@example.com", "StrongPass123!")]
    [InlineData(null, "valid@example.com", "StrongPass123!")]
    public void Register_EmptyUsername_ShouldHaveValidationError(string username, string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(username, email, password);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("ValidUser", "", "StrongPass123!")]
    [InlineData("ValidUser", "invalid-email", "StrongPass123!")]
    [InlineData("ValidUser", "no-at-sign.com", "StrongPass123!")]
    public void Register_InvalidEmail_ShouldHaveValidationError(string username, string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(username, email, password);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("ValidUser", "valid@example.com", "weak")]
    [InlineData("ValidUser", "valid@example.com", "")]
    [InlineData("ValidUser", "valid@example.com", "short")]
    [InlineData("ValidUser", "valid@example.com", "nouppercas1!")]
    [InlineData("ValidUser", "valid@example.com", "NOLOWERCASE1!")]
    [InlineData("ValidUser", "valid@example.com", "NoDigits!")]
    [InlineData("ValidUser", "valid@example.com", "NoSpecialChar1")]
    public void Register_WeakPassword_ShouldHaveValidationError(string username, string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(username, email, password);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("a", "valid@example.com", "StrongPass123!")]
    [InlineData("ab", "valid@example.com", "StrongPass123!")]
    public void Register_UsernameTooShort_ShouldHaveValidationError(string username, string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(username, email, password);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Register_UsernameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longUsername = new string('a', 51); // Max is 50
        var request = new RegisterRequest(longUsername, "valid@example.com", "StrongPass123!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
