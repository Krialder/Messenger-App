using MessengerContracts.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Hubs;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MessengerTests.ServiceTests;

/// <summary>
/// Unit tests for NotificationService - SignalR Hub functionality.
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly NotificationHub _hub;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationServiceTests()
    {
        // Setup mocks
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();

        // Setup logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<NotificationHub>();

        // Setup hub
        _hub = new NotificationHub(_logger)
        {
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object,
            Context = _mockContext.Object
        };

        // Setup default mock behaviors
        _mockClients.Setup(c => c.Others).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.OthersInGroup(It.IsAny<string>())).Returns(_mockClientProxy.Object);
    }

    private void SetupUserContext(string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(Guid.NewGuid().ToString());
    }

    // ========================================
    // CONNECTION LIFECYCLE TESTS
    // ========================================

    [Fact]
    public async Task OnConnectedAsync_ValidUser_NotifiesOthers()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "UserOnline",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ValidUser_NotifiesOthers()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "UserOffline",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_NoUserId_DoesNotNotify()
    {
        // Arrange - No user context

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default),
            Times.Never);
    }

    // ========================================
    // GROUP MANAGEMENT TESTS
    // ========================================

    [Fact]
    public async Task JoinConversation_ValidConversation_AddsToGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act
        await _hub.JoinConversation(conversationId);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync(
                It.IsAny<string>(),
                $"conversation_{conversationId}",
                default),
            Times.Once);
    }

    [Fact]
    public async Task JoinConversation_ValidConversation_NotifiesGroupMembers()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        var mockGroupClient = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.Group($"conversation_{conversationId}"))
            .Returns(mockGroupClient.Object);

        // Act
        await _hub.JoinConversation(conversationId);

        // Assert
        mockGroupClient.Verify(
            c => c.SendCoreAsync(
                "UserJoinedConversation",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    [Fact]
    public async Task LeaveConversation_ValidConversation_RemovesFromGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act
        await _hub.LeaveConversation(conversationId);

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync(
                It.IsAny<string>(),
                $"conversation_{conversationId}",
                default),
            Times.Once);
    }

    [Fact]
    public async Task LeaveConversation_ValidConversation_NotifiesGroupMembers()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        var mockGroupClient = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.Group($"conversation_{conversationId}"))
            .Returns(mockGroupClient.Object);

        // Act
        await _hub.LeaveConversation(conversationId);

        // Assert
        mockGroupClient.Verify(
            c => c.SendCoreAsync(
                "UserLeftConversation",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    // ========================================
    // TYPING INDICATOR TESTS
    // ========================================

    [Fact]
    public async Task NotifyTyping_UserStartsTyping_NotifiesGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        var mockOthersInGroup = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.OthersInGroup($"conversation_{conversationId}"))
            .Returns(mockOthersInGroup.Object);

        // Act
        await _hub.NotifyTyping(conversationId, true);

        // Assert
        mockOthersInGroup.Verify(
            c => c.SendCoreAsync(
                "UserTyping",
                It.Is<object[]>(args =>
                    args.Length > 0 &&
                    args[0] != null),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyTyping_UserStopsTyping_NotifiesGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        var mockOthersInGroup = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.OthersInGroup($"conversation_{conversationId}"))
            .Returns(mockOthersInGroup.Object);

        // Act
        await _hub.NotifyTyping(conversationId, false);

        // Assert
        mockOthersInGroup.Verify(
            c => c.SendCoreAsync(
                "UserTyping",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyTyping_NoUserId_DoesNotNotify()
    {
        // Arrange - No user context
        var conversationId = Guid.NewGuid().ToString();

        // Act
        await _hub.NotifyTyping(conversationId, true);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default),
            Times.Never);
    }

    // ========================================
    // MESSAGE DELIVERY CONFIRMATION TESTS
    // ========================================

    [Fact]
    public async Task ConfirmMessageDelivery_ValidMessage_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var messageId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act
        await _hub.ConfirmMessageDelivery(messageId);

        // Assert - Should not throw
        Assert.True(true);
    }

    [Fact]
    public async Task ConfirmMessageRead_ValidMessage_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var messageId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act
        await _hub.ConfirmMessageRead(messageId);

        // Assert - Should not throw
        Assert.True(true);
    }

    // ========================================
    // INTEGRATION SCENARIO TESTS
    // ========================================

    [Fact]
    public async Task CompleteConversationFlow_JoinTypingSendLeave_Works()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Step 1: Join conversation
        await _hub.JoinConversation(conversationId);

        _mockGroups.Verify(
            g => g.AddToGroupAsync(
                It.IsAny<string>(),
                $"conversation_{conversationId}",
                default),
            Times.Once);

        // Step 2: Start typing
        await _hub.NotifyTyping(conversationId, true);

        // Step 3: Stop typing
        await _hub.NotifyTyping(conversationId, false);

        // Step 4: Leave conversation
        await _hub.LeaveConversation(conversationId);

        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync(
                It.IsAny<string>(),
                $"conversation_{conversationId}",
                default),
            Times.Once);
    }

    [Fact]
    public async Task MultipleUsersInConversation_TypingNotifications_OnlyNotifiesOthers()
    {
        // Arrange
        var user1Id = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        SetupUserContext(user1Id);

        var mockOthersInGroup = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.OthersInGroup($"conversation_{conversationId}"))
            .Returns(mockOthersInGroup.Object);

        // Act - User 1 starts typing
        await _hub.NotifyTyping(conversationId, true);

        // Assert - Only others in group are notified, not the sender
        mockOthersInGroup.Verify(
            c => c.SendCoreAsync(
                "UserTyping",
                It.IsAny<object[]>(),
                default),
            Times.Once);

        // Verify sender is NOT notified directly
        _mockClients.Verify(
            c => c.User(user1Id),
            Times.Never);
    }

    [Fact]
    public async Task UserReconnection_OnConnected_NotifiesPresence()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act - Simulate reconnection
        await _hub.OnConnectedAsync();

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "UserOnline",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    [Fact]
    public async Task UserDisconnection_OnDisconnected_NotifiesPresence()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act - Simulate disconnection
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "UserOffline",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    // ========================================
    // ERROR HANDLING TESTS
    // ========================================

    [Fact]
    public async Task OnDisconnectedAsync_WithException_StillNotifies()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);
        var exception = new Exception("Test exception");

        // Act
        await _hub.OnDisconnectedAsync(exception);

        // Assert - Should still notify despite exception
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "UserOffline",
                It.Is<object[]>(o => o.Length > 0),
                default),
            Times.Once);
    }

    [Fact]
    public async Task JoinConversation_EmptyConversationId_HandlesGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act & Assert - Should not throw
        await _hub.JoinConversation(string.Empty);
    }

    [Fact]
    public async Task NotifyTyping_EmptyConversationId_HandlesGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupUserContext(userId);

        // Act & Assert - Should not throw
        await _hub.NotifyTyping(string.Empty, true);
    }
}
