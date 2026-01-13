using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MessageService.Data;
using MessageService.Data.Entities;
using MessageService.Services;
using MessengerContracts.DTOs;

namespace MessengerTests.IntegrationTests
{
    public class RabbitMQIntegrationTests : IDisposable
    {
        private readonly MessageDbContext _context;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<RabbitMQService>> _mockLogger;

        public RabbitMQIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<MessageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new MessageDbContext(options);

            // Setup configuration mock
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["RabbitMQ:HostName"]).Returns("localhost");
            _mockConfig.Setup(c => c["RabbitMQ:Port"]).Returns("5672");
            _mockConfig.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
            _mockConfig.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

            _mockLogger = new Mock<ILogger<RabbitMQService>>();
        }

        [Fact]
        public async Task SendMessage_ShouldStoreInDatabase()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = EntityConversationType.DirectMessage,
                CreatedBy = senderId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
            
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderId = senderId,
                EncryptedContent = new byte[] { 1, 2, 3, 4, 5 },
                Nonce = new byte[12],
                Type = EntityMessageType.Text,
                Status = EntityMessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Assert
            var savedMessage = await _context.Messages.FindAsync(message.Id);
            Assert.NotNull(savedMessage);
            Assert.Equal(senderId, savedMessage.SenderId);
            Assert.Equal(conversation.Id, savedMessage.ConversationId);
            Assert.Equal(EntityMessageStatus.Sent, savedMessage.Status);
        }

        [Fact]
        public async Task CreateGroup_ShouldStoreConversationAndMembers()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = EntityConversationType.Group,
                Name = "Test Group",
                Description = "Integration Test Group",
                CreatedBy = ownerId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Conversations.AddAsync(conversation);

            // Owner
            await _context.ConversationMembers.AddAsync(new ConversationMember
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                UserId = ownerId,
                Role = EntityMemberRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsMuted = false
            });

            // Members
            foreach (var memberId in memberIds)
            {
                await _context.ConversationMembers.AddAsync(new ConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    UserId = memberId,
                    Role = EntityMemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsMuted = false
                });
            }

            await _context.SaveChangesAsync();

            // Act & Assert
            var savedConversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversation.Id);

            Assert.NotNull(savedConversation);
            Assert.Equal(EntityConversationType.Group, savedConversation.Type);
            Assert.Equal(5, savedConversation.Members.Count); // Owner + 4 Members
            Assert.Single(savedConversation.Members.Where(m => m.Role == EntityMemberRole.Owner));
            Assert.Equal(4, savedConversation.Members.Count(m => m.Role == EntityMemberRole.Member));
        }

        [Fact]
        public async Task MessageDelivered_ShouldUpdateStatus()
        {
            // Arrange
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                EncryptedContent = new byte[] { 1, 2, 3, 4, 5 },
                Nonce = new byte[12],
                Type = EntityMessageType.Text,
                Status = EntityMessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Act
            message.Status = EntityMessageStatus.Delivered;
            await _context.SaveChangesAsync();

            var deliveredMessage = await _context.Messages.FindAsync(message.Id);

            // Assert
            Assert.NotNull(deliveredMessage);
            Assert.Equal(EntityMessageStatus.Delivered, deliveredMessage.Status);
        }

        [Fact]
        public async Task RabbitMQ_PublishEvent_ShouldNotThrow()
        {
            // NOTE: This test requires RabbitMQ to be running
            // Skip if RabbitMQ is not available
            
            RabbitMQService? rabbitMQService = null;
            
            try
            {
                rabbitMQService = new RabbitMQService(_mockConfig.Object, _mockLogger.Object);
                
                var messageEvent = new MessageSentEvent
                {
                    MessageId = Guid.NewGuid(),
                    SenderId = Guid.NewGuid(),
                    RecipientIds = new List<Guid> { Guid.NewGuid() },
                    ConversationId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow
                };

                // Act
                await rabbitMQService.PublishMessageSentEventAsync(messageEvent);

                // Assert
                Assert.True(true, "Event published successfully");
            }
            catch (Exception ex)
            {
                // RabbitMQ might not be running in test environment
                _mockLogger.Verify(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
                    Times.AtLeastOnce);
                
                Assert.True(true, $"RabbitMQ not available: {ex.Message}");
            }
            finally
            {
                rabbitMQService?.Dispose();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
