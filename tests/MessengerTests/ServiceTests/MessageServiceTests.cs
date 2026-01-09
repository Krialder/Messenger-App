// ========================================
// MESSAGE SERVICE TESTS
// Status: ✅ IMPLEMENTED - Sprint 5
// ========================================

using MessageService.Controllers;
using MessageService.Data;
using MessageService.Data.Entities;
using MessageService.Services;
using MessengerContracts.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;
using DtoConversationType = MessengerContracts.DTOs.ConversationType;
using DtoMemberRole = MessengerContracts.DTOs.MemberRole;
using DtoMessageType = MessengerContracts.DTOs.MessageType;

namespace MessengerTests.ServiceTests;

/// <summary>
/// Unit tests for MessageService - Conversations, Messages, Groups.
/// </summary>
public class MessageServiceTests : IDisposable
{
    private readonly MessageDbContext _context;
    private readonly MessagesController _messagesController;
    private readonly GroupsController _groupsController;
    private readonly Mock<IRabbitMQService> _mockRabbitMQ;
    private readonly ILogger<MessagesController> _messagesLogger;
    private readonly ILogger<GroupsController> _groupsLogger;
    private readonly Guid _testUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly Guid _otherUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public MessageServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<MessageDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MessageDbContext(options);

        // Setup mocks
        _mockRabbitMQ = new Mock<IRabbitMQService>();

        // Setup loggers
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _messagesLogger = loggerFactory.CreateLogger<MessagesController>();
        _groupsLogger = loggerFactory.CreateLogger<GroupsController>();

        // Setup controllers
        _messagesController = new MessagesController(_context, _mockRabbitMQ.Object, _messagesLogger);
        _groupsController = new GroupsController(_context, _groupsLogger);

        // Setup user context
        SetupUserContext(_messagesController, _testUserId);
        SetupUserContext(_groupsController, _testUserId);

        // Seed test data
        SeedTestData();
    }

    private void SetupUserContext(ControllerBase controller, Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim("userId", userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SeedTestData()
    {
        var directConversation = new Conversation
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Type = EntityConversationType.DirectMessage,
            CreatedBy = _testUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var member1 = new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = directConversation.Id,
            UserId = _testUserId,
            Role = EntityMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        var member2 = new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = directConversation.Id,
            UserId = _otherUserId,
            Role = EntityMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(directConversation);
        _context.ConversationMembers.AddRange(member1, member2);

        var message = new Message
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            ConversationId = directConversation.Id,
            SenderId = _testUserId,
            EncryptedContent = new byte[] { 1, 2, 3, 4, 5 },
            Nonce = new byte[] { 1, 2, 3 },
            CreatedAt = DateTime.UtcNow,
            Status = EntityMessageStatus.Sent,
            Type = EntityMessageType.Text
        };

        _context.Messages.Add(message);
        _context.SaveChanges();
    }

    // ========================================
    // MESSAGE SENDING TESTS
    // ========================================

    [Fact]
    public async Task SendMessage_ValidMessage_ReturnsSuccess()
    {
        var request = new SendMessageRequest(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            new byte[] { 1, 2, 3, 4, 5, 6 },
            new byte[] { 7, 8, 9 },
            null,
            null,
            DtoMessageType.Text,
            null
        );

        // Act
        var result = await _messagesController.SendMessage(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify message in database
        var messages = await _context.Messages
            .Where(m => m.ConversationId == request.ConversationId)
            .ToListAsync();

        Assert.Equal(2, messages.Count); // 1 seeded + 1 new

        // Verify RabbitMQ was called
        _mockRabbitMQ.Verify(
            r => r.PublishMessageSentEventAsync(
                It.IsAny<MessageSentEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_UserNotInConversation_ReturnsForbidden()
    {
        // Arrange - Create conversation where user is not a member
        var privateConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = EntityConversationType.DirectMessage,
            CreatedBy = _otherUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var privateMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = privateConversation.Id,
            UserId = _otherUserId,
            Role = EntityMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(privateConversation);
        _context.ConversationMembers.Add(privateMember);
        await _context.SaveChangesAsync();

        var request = new SendMessageRequest(
            privateConversation.Id,
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6 },
            null
        );

        // Act
        var result = await _messagesController.SendMessage(request);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task SendMessage_NonExistentConversation_ReturnsNotFound()
    {
        // Arrange
        var request = new SendMessageRequest(
            Guid.NewGuid(),
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6 },
            null
        );

        // Act
        var result = await _messagesController.SendMessage(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ========================================
    // MESSAGE RETRIEVAL TESTS
    // ========================================

    [Fact]
    public async Task GetConversationMessages_ValidConversation_ReturnsMessages()
    {
        // Arrange
        var conversationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        // Act
        var result = await _messagesController.GetConversationMessages(conversationId, 1, 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        
        var messagesProp = response.GetType().GetProperty("messages");
        var messages = messagesProp?.GetValue(response) as IEnumerable<MessageDto>;
        
        Assert.NotNull(messages);
        Assert.Single(messages);
    }

    [Fact]
    public async Task GetConversationMessages_UserNotMember_ReturnsForbidden()
    {
        // Arrange - Create private conversation
        var privateConv = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = EntityConversationType.DirectMessage,
            CreatedBy = _otherUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var privateMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = privateConv.Id,
            UserId = _otherUserId,
            Role = EntityMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(privateConv);
        _context.ConversationMembers.Add(privateMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _messagesController.GetConversationMessages(privateConv.Id, 1, 50);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetConversationMessages_Pagination_Works()
    {
        // Arrange - Add more messages
        var conversationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        
        for (int i = 0; i < 5; i++)
        {
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = _testUserId,
                EncryptedContent = new byte[] { (byte)i },
                Nonce = new byte[] { (byte)i },
                CreatedAt = DateTime.UtcNow.AddMinutes(i),
                Status = EntityMessageStatus.Sent,
                Type = EntityMessageType.Text
            };
            _context.Messages.Add(msg);
        }
        await _context.SaveChangesAsync();

        // Act - Get page 1 with size 3
        var result = await _messagesController.GetConversationMessages(conversationId, 1, 3);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        var messagesProp = response.GetType().GetProperty("messages");
        var messages = messagesProp?.GetValue(response) as IEnumerable<MessageDto>;
        
        Assert.NotNull(messages);
        Assert.Equal(3, messages.Count());
        
        var hasMoreProp = response.GetType().GetProperty("hasMore");
        var hasMore = (bool?)hasMoreProp?.GetValue(response);
        Assert.True(hasMore);
    }

    // ========================================
    // MESSAGE DELETION TESTS
    // ========================================

    [Fact]
    public async Task DeleteMessage_ValidMessage_MarksAsDeleted()
    {
        // Arrange
        var messageId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        // Act
        var result = await _messagesController.DeleteMessage(messageId);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify soft delete
        var deletedMessage = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId);
        
        Assert.NotNull(deletedMessage);
        Assert.True(deletedMessage.IsDeleted);
        Assert.NotNull(deletedMessage.DeletedAt);
    }

    [Fact]
    public async Task DeleteMessage_OtherUserMessage_ReturnsForbidden()
    {
        // Arrange - Create message from other user
        var otherUserMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            SenderId = _otherUserId,
            EncryptedContent = new byte[] { 1, 2, 3 },
            Nonce = new byte[] { 4, 5, 6 },
            CreatedAt = DateTime.UtcNow,
            Status = EntityMessageStatus.Sent,
            Type = EntityMessageType.Text
        };

        _context.Messages.Add(otherUserMessage);
        await _context.SaveChangesAsync();

        // Act
        var result = await _messagesController.DeleteMessage(otherUserMessage.Id);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    // ========================================
    // GROUP CONVERSATION TESTS
    // ========================================

    [Fact]
    public async Task CreateGroup_ValidRequest_CreatesGroup()
    {
        var request = new CreateGroupRequest(
            "Test Group",
            "Test Description",
            new List<Guid> { _otherUserId }
        );

        var result = await _groupsController.CreateGroup(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var conversation = Assert.IsType<ConversationDto>(createdResult.Value);
        
        Assert.Equal("Test Group", conversation.Name);
        Assert.Equal(DtoConversationType.Group, conversation.Type);

        var members = await _context.ConversationMembers
            .Where(m => m.ConversationId == conversation.Id)
            .ToListAsync();

        Assert.Equal(2, members.Count);
    }

    [Fact]
    public async Task RemoveMemberFromGroup_ValidRequest_RemovesMember()
    {
        var group = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = EntityConversationType.Group,
            Name = "Test Group",
            CreatedBy = _testUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var ownerMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = group.Id,
            UserId = _testUserId,
            Role = EntityMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        var regularMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = group.Id,
            UserId = _otherUserId,
            Role = EntityMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(group);
        _context.ConversationMembers.AddRange(ownerMember, regularMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _groupsController.RemoveMember(group.Id, _otherUserId);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify removal (soft delete - LeftAt is set)
        var member = await _context.ConversationMembers
            .FirstOrDefaultAsync(m => m.Id == regularMember.Id);
        
        Assert.NotNull(member);
        Assert.NotNull(member.LeftAt);
    }

    // ========================================
    // UNREAD MESSAGES TESTS
    // ========================================

    [Fact]
    public async Task GetUnreadMessages_HasUnread_ReturnsCount()
    {
        // Act
        var result = await _messagesController.GetUnreadMessages();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task MarkConversationAsRead_UpdatesReadTimestamp()
    {
        // Arrange
        var conversationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        // Act
        var result = await _messagesController.MarkConversationAsRead(conversationId);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify timestamp updated
        var member = await _context.ConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == _testUserId);
        
        Assert.NotNull(member);
        Assert.NotNull(member.LastReadAt);
        
        // Verify RabbitMQ was called
        _mockRabbitMQ.Verify(
            r => r.PublishMessageReadEventAsync(
                It.IsAny<MessageReadEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
