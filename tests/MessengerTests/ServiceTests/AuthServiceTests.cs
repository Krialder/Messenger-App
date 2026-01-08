// ========================================
// PSEUDO-CODE - Sprint 2: Auth Service Tests
// Status: 🔶 Test-Struktur definiert
// ========================================

using Xunit;

namespace SecureMessenger.Tests.ServiceTests;

public class AuthServiceTests
{
    // TODO-SPRINT-2: Setup mock dependencies
    
    [Fact]
    public async Task Register_ValidInput_ReturnsSuccess()
    {
        // PSEUDO: Arrange - Create mock repositories
        // PSEUDO: Act - Call RegisterUser
        // PSEUDO: Assert - Verify user created, salt generated
        
        Assert.True(false, "NOT IMPLEMENTED - Sprint 2");
    }
    
    [Fact]
    public async Task Register_DuplicateUsername_ThrowsException()
    {
        // PSEUDO: Arrange - Mock repository with existing user
        // PSEUDO: Act & Assert - Expect ConflictException
        
        Assert.True(false, "NOT IMPLEMENTED - Sprint 2");
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken()
    {
        // PSEUDO: Arrange - Mock user with hashed password
        // PSEUDO: Act - Call Login
        // PSEUDO: Assert - JWT token returned, not null
        
        Assert.True(false, "NOT IMPLEMENTED - Sprint 2");
    }
    
    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // PSEUDO: Arrange - Mock user
        // PSEUDO: Act - Login with wrong password
        // PSEUDO: Assert - Unauthorized exception
        
        Assert.True(false, "NOT IMPLEMENTED - Sprint 2");
    }
}
