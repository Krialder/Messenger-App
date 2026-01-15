using AuthService.Controllers;
using AuthService.Data;
using AuthService.Data.Entities;
using AuthService.Services;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Xunit;

namespace MessengerTests.IntegrationTests;

public class MFAControllerIntegrationTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly MFAController _controller;
    private readonly IMfaService _mfaService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<MFAController> _logger;

    public MFAControllerIntegrationTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);

        // Setup services
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<MFAController>();
        
        _passwordHasher = new Argon2PasswordHasher();
        _mfaService = new MFAService(_context, _passwordHasher);

        // Set TOTP encryption key for tests
        Environment.SetEnvironmentVariable("TOTP_ENCRYPTION_KEY", 
            "TestEncryptionKey32CharsMinimumForAES256SecurityRequirement123");

        // Setup controller
        _controller = new MFAController(_context, _mfaService, _logger);
    }

    [Fact]
    public async Task EnableTotp_WhenCalledTwice_ShouldRemovePendingMethod()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Act - Call EnableTotp twice
        var response1 = await _controller.EnableTotp();
        var response2 = await _controller.EnableTotp();

        // Assert
        var okResult1 = Assert.IsType<OkObjectResult>(response1);
        var okResult2 = Assert.IsType<OkObjectResult>(response2);
        
        Assert.NotNull(okResult1.Value);
        Assert.NotNull(okResult2.Value);

        // Verify only one pending TOTP method exists
        var pendingMethods = await _context.MfaMethods
            .Where(m => m.UserId == userId && m.MethodType == "totp" && !m.IsActive)
            .CountAsync();

        Assert.Equal(1, pendingMethods);
    }

    [Fact]
    public async Task DisableMfaMethod_WhenLastActiveMethod_ShouldDisableMfaForUser()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Enable TOTP
        var enableResponse = await _controller.EnableTotp();
        var okResult = Assert.IsType<OkObjectResult>(enableResponse);
        
        // Get the response value dynamically
        dynamic? enableData = okResult.Value;
        Assert.NotNull(enableData);

        // Manually activate the TOTP method (simulating successful verification)
        var totpMethod = await _context.MfaMethods
            .FirstAsync(m => m.UserId == userId && m.MethodType == "totp");
        totpMethod.IsActive = true;
        user.MfaEnabled = true;
        await _context.SaveChangesAsync();

        // Act - Disable the only MFA method
        var deleteResponse = await _controller.DisableMfaMethod(totpMethod.Id);

        // Assert
        Assert.IsType<NoContentResult>(deleteResponse);

        // Verify MFA is disabled for user
        var updatedUser = await _context.Users.FindAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.MfaEnabled);
    }

    [Fact]
    public async Task VerifyTotpSetup_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        await _controller.EnableTotp();

        // Act - Verify with invalid code format
        var request = new VerifyTotpSetupRequest(Code: "abcdef"); // Invalid: contains letters
        var response = await _controller.VerifyTotpSetup(request);

        // Assert - FluentValidation should catch this
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task VerifyTotpSetup_WithInvalidNumericCode_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        var enableResponse = await _controller.EnableTotp();
        var okResult = Assert.IsType<OkObjectResult>(enableResponse);

        // Act - Verify with wrong TOTP code
        var request = new VerifyTotpSetupRequest(Code: "000000"); // Valid format but wrong code
        var response = await _controller.VerifyTotpSetup(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateRecoveryCodes_ShouldNotDuplicateRemoval()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Act - Generate recovery codes twice
        var response1 = await _controller.GenerateRecoveryCodes();
        var response2 = await _controller.GenerateRecoveryCodes();

        // Assert
        var okResult1 = Assert.IsType<OkObjectResult>(response1);
        var okResult2 = Assert.IsType<OkObjectResult>(response2);
        
        Assert.NotNull(okResult1.Value);
        Assert.NotNull(okResult2.Value);

        // Verify old codes are removed (only 10 recovery codes should exist)
        var codeCount = await _context.RecoveryCodes
            .Where(r => r.UserId == userId && !r.Used)
            .CountAsync();

        Assert.Equal(10, codeCount);
    }

    [Fact]
    public async Task GetMfaMethods_ShouldReturnUserMethods()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Enable TOTP
        await _controller.EnableTotp();

        // Act
        var response = await _controller.GetMfaMethods();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var methods = Assert.IsAssignableFrom<List<MfaMethodDto>>(okResult.Value);
        
        Assert.NotEmpty(methods);
        Assert.Single(methods);
        Assert.Equal("totp", methods[0].MethodType);
    }

    [Fact]
    public async Task EnableTotp_WhenAlreadyActive_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Enable and activate TOTP
        await _controller.EnableTotp();
        var totpMethod = await _context.MfaMethods
            .FirstAsync(m => m.UserId == userId && m.MethodType == "totp");
        totpMethod.IsActive = true;
        await _context.SaveChangesAsync();

        // Act - Try to enable again
        var response = await _controller.EnableTotp();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal("TOTP already enabled", problemDetails.Title);
    }

    [Fact]
    public async Task VerifyTotpSetup_WhenNoPendingSetup_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Act - Verify without enabling first
        var request = new VerifyTotpSetupRequest(Code: "123456");
        var response = await _controller.VerifyTotpSetup(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal("No pending TOTP setup", problemDetails.Title);
    }

    [Fact]
    public async Task DisableMfaMethod_WithOtherActiveMethod_ShouldNotDisableMfa()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Create two active TOTP methods
        var method1 = new MfaMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = "totp",
            TotpSecret = "TESTSECRET123456",
            IsActive = true,
            IsPrimary = true,
            FriendlyName = "Method 1",
            CreatedAt = DateTime.UtcNow
        };

        var method2 = new MfaMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = "totp",
            TotpSecret = "TESTSECRET654321",
            IsActive = true,
            IsPrimary = false,
            FriendlyName = "Method 2",
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaMethods.AddRange(method1, method2);
        user.MfaEnabled = true;
        await _context.SaveChangesAsync();

        // Act - Disable first method
        var response = await _controller.DisableMfaMethod(method1.Id);

        // Assert
        Assert.IsType<NoContentResult>(response);

        // Verify MFA is still enabled because another active method exists
        var updatedUser = await _context.Users.FindAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.True(updatedUser.MfaEnabled);
    }

    [Fact]
    public async Task EnableTotp_ShouldGenerateRecoveryCodes()
    {
        // Arrange
        var (userId, user) = await CreateTestUser();
        SetupControllerContext(userId);

        // Act
        var response = await _controller.EnableTotp();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        
        // Verify response structure
        Assert.NotNull(okResult.Value);
        
        // Use reflection to access properties since we can't directly cast to EnableTotpResponse
        var responseType = okResult.Value.GetType();
        var secretProp = responseType.GetProperty("Secret");
        var qrCodeProp = responseType.GetProperty("QrCodeUrl");
        var backupCodesProp = responseType.GetProperty("BackupCodes");
        
        Assert.NotNull(secretProp);
        Assert.NotNull(qrCodeProp);
        Assert.NotNull(backupCodesProp);
        
        var secret = secretProp.GetValue(okResult.Value) as string;
        var qrCode = qrCodeProp.GetValue(okResult.Value) as string;
        var backupCodes = backupCodesProp.GetValue(okResult.Value) as List<string>;
        
        Assert.NotEmpty(secret);
        Assert.NotEmpty(qrCode);
        Assert.NotNull(backupCodes);
        Assert.Equal(10, backupCodes.Count);

        // Verify recovery codes in database
        var codeCount = await _context.RecoveryCodes
            .Where(r => r.UserId == userId)
            .CountAsync();
        Assert.Equal(10, codeCount);
    }

    private async Task<(Guid userId, User user)> CreateTestUser()
    {
        var userId = Guid.NewGuid();
        var salt = new byte[32];
        new Random().NextBytes(salt);

        var user = new User
        {
            Id = userId,
            Username = $"testuser_{userId:N}",
            Email = $"test_{userId:N}@example.com",
            PasswordHash = _passwordHasher.HashPassword("TestPassword123!"),
            MasterKeySalt = salt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailVerified = true,
            MfaEnabled = false,
            AccountStatus = "active"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (userId, user);
    }

    private void SetupControllerContext(Guid userId)
    {
        // Setup HttpContext with authenticated user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"testuser_{userId:N}")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
