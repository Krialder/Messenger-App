using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MessengerContracts.DTOs;
using System.Security.Claims;
using UserService.Controllers;
using UserService.Data;
using UserService.Data.Entities;
using Xunit;

namespace MessengerTests.ServiceTests;

/// <summary>
/// Unit tests for UserService API endpoints.
/// </summary>
public class UserServiceTests : IDisposable
{
    private readonly UserDbContext _context;
    private readonly UsersController _controller;
    private readonly ILogger<UsersController> _logger;

    public UserServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserDbContext(options);

        // Setup logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UsersController>();

        // Setup controller
        _controller = new UsersController(_context, _logger);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user1 = new UserProfile
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Username = "testuser1",
            Email = "test1@example.com",
            MasterKeySalt = new byte[32],
            DisplayName = "Test User 1",
            Bio = "Test bio 1",
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new UserProfile
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Username = "testuser2",
            Email = "test2@example.com",
            MasterKeySalt = new byte[32],
            DisplayName = "Test User 2",
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user3 = new UserProfile
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Username = "inactiveuser",
            Email = "inactive@example.com",
            MasterKeySalt = new byte[32],
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.AddRange(user1, user2, user3);

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            ContactUserId = user2.Id,
            Nickname = "My Friend",
            IsBlocked = false,
            AddedAt = DateTime.UtcNow
        };

        _context.Contacts.Add(contact);
        _context.SaveChanges();
    }

    private void SetupUserContext(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // ========================================
    // PROFILE MANAGEMENT TESTS
    // ========================================

    [Fact]
    public async Task GetProfile_ValidUserId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.GetProfile(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("testuser1", user.Username);
        Assert.Equal("Test User 1", user.DisplayName);
        Assert.True(user.EmailVerified);
    }

    [Fact]
    public async Task GetProfile_InactiveUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        SetupUserContext(userId);

        // Act
        var result = await _controller.GetProfile(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetProfile_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        SetupUserContext(userId);

        // Act
        var result = await _controller.GetProfile(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMyProfile_ReturnsCurrentUser()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.GetMyProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("testuser1", user.Username);
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_UpdatesProfile()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var request = new MessengerContracts.DTOs.UpdateProfileRequest
        {
            DisplayName = "Updated Name",
            Bio = "Updated bio",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        // Act
        var result = await _controller.UpdateProfile(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("Updated Name", user.DisplayName);
        Assert.Equal("Updated bio", user.Bio);
        Assert.Equal("https://example.com/avatar.jpg", user.AvatarUrl);

        // Verify in database
        var dbUser = await _context.UserProfiles.FindAsync(userId);
        Assert.NotNull(dbUser);
        Assert.Equal("Updated Name", dbUser.DisplayName);
        Assert.NotNull(dbUser.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProfile_BioTooLong_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var request = new MessengerContracts.DTOs.UpdateProfileRequest
        {
            Bio = new string('x', 501) // 501 characters
        };

        // Act
        var result = await _controller.UpdateProfile(request);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task GetMasterKeySalt_ValidUser_ReturnsSalt()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _controller.GetSalt(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var saltData = okResult.Value;
        var userIdProp = saltData.GetType().GetProperty("userId")?.GetValue(saltData);
        Assert.Equal(userId, userIdProp);
    }

    // ========================================
    // USER SEARCH TESTS
    // ========================================

    [Fact]
    public async Task SearchUsers_ValidQuery_ReturnsMatchingUsers()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.Search("testuser2", 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(okResult.Value);
        var userList = users.ToList();
        
        Assert.Single(userList);
        Assert.Equal("testuser2", userList[0].Username);
        Assert.True(userList[0].IsContact); // user2 is a contact of user1
    }

    [Fact]
    public async Task SearchUsers_QueryTooShort_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.Search("a", 10);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task SearchUsers_EmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.Search("", 10);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task SearchUsers_ExcludesCurrentUser()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.Search("testuser", 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(okResult.Value);
        var userList = users.ToList();
        
        Assert.DoesNotContain(userList, u => u.Id == userId);
    }

    [Fact]
    public async Task SearchUsers_ExcludesInactiveUsers()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.Search("inactive", 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(okResult.Value);
        var userList = users.ToList();
        
        Assert.Empty(userList);
    }

    // ========================================
    // CONTACT MANAGEMENT TESTS
    // ========================================

    [Fact]
    public async Task AddContact_ValidRequest_AddsContact()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var request = new AddContactRequest
        {
            ContactUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Nickname = "New Friend"
        };

        // Act
        var result = await _controller.AddContact(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify in database
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ContactUserId == request.ContactUserId);
        
        Assert.NotNull(contact);
        Assert.Equal("New Friend", contact.Nickname);
    }

    [Fact]
    public async Task AddContact_AddSelf_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var request = new AddContactRequest
        {
            ContactUserId = userId
        };

        // Act
        var result = await _controller.AddContact(request);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task AddContact_DuplicateContact_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var request = new AddContactRequest
        {
            ContactUserId = Guid.Parse("22222222-2222-2222-2222-222222222222")
        };

        // Act
        var result = await _controller.AddContact(request);

        // Assert
        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task GetContacts_ReturnsUserContacts()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.GetContacts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var contacts = Assert.IsAssignableFrom<IEnumerable<ContactDto>>(okResult.Value);
        var contactList = contacts.ToList();
        
        Assert.Single(contactList);
        Assert.Equal("testuser2", contactList[0].Username);
        Assert.Equal("My Friend", contactList[0].Nickname);
    }

    [Fact]
    public async Task RemoveContact_ValidContact_RemovesContact()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.UserId == userId);
        
        Assert.NotNull(contact);

        // Act
        var result = await _controller.RemoveContact(contact.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify deletion
        var deletedContact = await _context.Contacts.FindAsync(contact.Id);
        Assert.Null(deletedContact);
    }

    [Fact]
    public async Task RemoveContact_NonExistentContact_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        // Act
        var result = await _controller.RemoveContact(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveContact_OtherUsersContact_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        SetupUserContext(userId);

        var otherUsersContact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.UserId == Guid.Parse("11111111-1111-1111-1111-111111111111"));
        
        Assert.NotNull(otherUsersContact);

        // Act
        var result = await _controller.RemoveContact(otherUsersContact.Id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // ========================================
    // DSGVO / ACCOUNT DELETION TESTS
    // ========================================

    [Fact]
    public async Task DeleteAccount_ValidRequest_SchedulesDeletion()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var request = new MessengerContracts.DTOs.DeleteAccountRequest
        {
            Password = "test-password",
            Reason = "Testing deletion"
        };

        // Act
        var result = await _controller.DeleteAccount(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify in database
        var user = await _context.UserProfiles.FindAsync(userId);
        Assert.NotNull(user);
        Assert.NotNull(user.DeleteScheduledAt);
        Assert.True(user.DeleteScheduledAt > DateTime.UtcNow.AddDays(29));
        Assert.True(user.DeleteScheduledAt < DateTime.UtcNow.AddDays(31));
    }

    [Fact]
    public async Task DeleteAccount_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        SetupUserContext(userId);

        var request = new MessengerContracts.DTOs.DeleteAccountRequest
        {
            Password = "test-password"
        };

        // Act
        var result = await _controller.DeleteAccount(request);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // ========================================
    // INTEGRATION TESTS
    // ========================================

    [Fact]
    public async Task FullContactWorkflow_AddSearchRemove_Works()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        SetupUserContext(userId);

        var newContactId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Step 1: Search for user
        var searchResult = await _controller.Search("inactive", 10);
        var searchOk = Assert.IsType<OkObjectResult>(searchResult);
        var searchUsers = Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(searchOk.Value);
        
        // Should be empty because user is inactive
        Assert.Empty(searchUsers);

        // Make user active for this test
        var targetUser = await _context.UserProfiles.FindAsync(newContactId);
        Assert.NotNull(targetUser);
        targetUser.IsActive = true;
        await _context.SaveChangesAsync();

        // Step 2: Search again
        var searchResult2 = await _controller.Search("inactive", 10);
        var searchOk2 = Assert.IsType<OkObjectResult>(searchResult2);
        var searchUsers2 = Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(searchOk2.Value).ToList();
        Assert.Single(searchUsers2);
        Assert.False(searchUsers2[0].IsContact);

        // Step 3: Add as contact
        var addRequest = new AddContactRequest { ContactUserId = newContactId, Nickname = "Test Contact" };
        var addResult = await _controller.AddContact(addRequest);
        Assert.IsType<OkObjectResult>(addResult);

        // Step 4: Verify in contact list
        var contactsResult = await _controller.GetContacts();
        var contactsOk = Assert.IsType<OkObjectResult>(contactsResult);
        var contacts = Assert.IsAssignableFrom<IEnumerable<ContactDto>>(contactsOk.Value).ToList();
        Assert.Equal(2, contacts.Count); // Original contact + new one
        
        var newContact = contacts.FirstOrDefault(c => c.ContactUserId == newContactId);
        Assert.NotNull(newContact);
        Assert.Equal("Test Contact", newContact.Nickname);

        // Step 5: Search shows IsContact = true
        var searchResult3 = await _controller.Search("inactive", 10);
        var searchOk3 = Assert.IsType<OkObjectResult>(searchResult3);
        var searchUsers3 = Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(searchOk3.Value).ToList();
        Assert.Single(searchUsers3);
        Assert.True(searchUsers3[0].IsContact);

        // Step 6: Remove contact
        var removeResult = await _controller.RemoveContact(newContact.Id);
        Assert.IsType<NoContentResult>(removeResult);

        // Step 7: Verify removal
        var contactsResult2 = await _controller.GetContacts();
        var contactsOk2 = Assert.IsType<OkObjectResult>(contactsResult2);
        var contacts2 = Assert.IsAssignableFrom<IEnumerable<ContactDto>>(contactsOk2.Value).ToList();
        Assert.Single(contacts2); // Only original contact remains
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
