using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using AuditLogService.Data;
using AuditLogService.Data.Entities;
using AuditLogService.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MessengerTests.ServiceTests
{
    public class AuditLogServiceTests : IDisposable
    {
        private readonly AuditDbContext _context;
        private readonly AuditController _controller;
        private readonly Mock<ILogger<AuditController>> _mockLogger;

        public AuditLogServiceTests()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AuditDbContext(options);

            _mockLogger = new Mock<ILogger<AuditController>>();
            _controller = new AuditController(_context, _mockLogger.Object);
        }

        // ========================================
        // Create Audit Log Tests
        // ========================================

        [Fact]
        public async Task CreateAuditLog_ValidRequest_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAuditLogRequest
            {
                UserId = userId,
                Action = "user_login",
                Resource = "AuthService",
                Details = "{\"ip\":\"192.168.1.1\"}",
                IpAddress = "192.168.1.1",
                Severity = "Info"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreateAuditLog(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var auditLogDto = Assert.IsType<AuditLogDto>(createdResult.Value);

            Assert.Equal(userId, auditLogDto.UserId);
            Assert.Equal("user_login", auditLogDto.Action);
            Assert.Equal("AuthService", auditLogDto.Resource);
            Assert.Equal("Info", auditLogDto.Severity);

            var savedLog = await _context.AuditLogs.FindAsync(auditLogDto.Id);
            Assert.NotNull(savedLog);
        }

        [Fact]
        public async Task CreateAuditLog_DefaultSeverity_IsInfo()
        {
            // Arrange
            var request = new CreateAuditLogRequest
            {
                UserId = Guid.NewGuid(),
                Action = "test_action",
                Resource = "TestService"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreateAuditLog(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var auditLogDto = Assert.IsType<AuditLogDto>(createdResult.Value);
            Assert.Equal("Info", auditLogDto.Severity);
        }

        [Fact]
        public async Task CreateAuditLog_CriticalSeverity_Success()
        {
            // Arrange
            var request = new CreateAuditLogRequest
            {
                UserId = Guid.NewGuid(),
                Action = "security_breach_detected",
                Resource = "SecurityService",
                Severity = "Critical"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreateAuditLog(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var auditLogDto = Assert.IsType<AuditLogDto>(createdResult.Value);
            Assert.Equal("Critical", auditLogDto.Severity);
        }

        // ========================================
        // Get Audit Logs (Admin) Tests
        // ========================================

        [Fact]
        public async Task GetAuditLogs_FilterByUserId_ReturnsCorrectLogs()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Create logs for target user
            await CreateTestAuditLog(targetUserId, "action1");
            await CreateTestAuditLog(targetUserId, "action2");

            // Create log for other user
            await CreateTestAuditLog(otherUserId, "action3");

            SetupAdminUser();

            // Act
            var result = await _controller.GetAuditLogs(userId: targetUserId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuditLogResponse>(okResult.Value);
            Assert.Equal(2, response.Logs.Count);
            Assert.All(response.Logs, log => Assert.Equal(targetUserId, log.UserId));
        }

        [Fact]
        public async Task GetAuditLogs_FilterByAction_ReturnsCorrectLogs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            await CreateTestAuditLog(userId, "user_login");
            await CreateTestAuditLog(userId, "user_login");
            await CreateTestAuditLog(userId, "user_logout");

            SetupAdminUser();

            // Act
            var result = await _controller.GetAuditLogs(action: "user_login");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuditLogResponse>(okResult.Value);
            Assert.Equal(2, response.Logs.Count);
            Assert.All(response.Logs, log => Assert.Contains("user_login", log.Action));
        }

        [Fact]
        public async Task GetAuditLogs_FilterBySeverity_ReturnsCorrectLogs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            await CreateTestAuditLog(userId, "action1", severity: "Info");
            await CreateTestAuditLog(userId, "action2", severity: "Warning");
            await CreateTestAuditLog(userId, "action3", severity: "Error");

            SetupAdminUser();

            // Act
            var result = await _controller.GetAuditLogs(severity: "Error");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuditLogResponse>(okResult.Value);
            Assert.Single(response.Logs);
            Assert.Equal("Error", response.Logs[0].Severity);
        }

        [Fact]
        public async Task GetAuditLogs_FilterByDateRange_ReturnsCorrectLogs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var tomorrow = DateTime.UtcNow.AddDays(1);

            await CreateTestAuditLog(userId, "old_action", timestamp: DateTime.UtcNow.AddDays(-2));
            await CreateTestAuditLog(userId, "current_action", timestamp: DateTime.UtcNow);
            await CreateTestAuditLog(userId, "future_action", timestamp: DateTime.UtcNow.AddDays(2));

            SetupAdminUser();

            // Act
            var result = await _controller.GetAuditLogs(startDate: yesterday, endDate: tomorrow);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuditLogResponse>(okResult.Value);
            Assert.Single(response.Logs);
            Assert.Equal("current_action", response.Logs[0].Action);
        }

        [Fact]
        public async Task GetAuditLogs_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            for (int i = 0; i < 10; i++)
            {
                await CreateTestAuditLog(userId, $"action_{i}");
            }

            SetupAdminUser();

            // Act - Get page 2 with 3 items per page
            var result = await _controller.GetAuditLogs(page: 2, pageSize: 3);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuditLogResponse>(okResult.Value);
            Assert.Equal(3, response.Logs.Count);
            Assert.Equal(10, response.TotalCount);
            Assert.Equal(2, response.Page);
            Assert.Equal(3, response.PageSize);
        }

        // ========================================
        // Get Own Audit Logs (User) Tests
        // ========================================

        [Fact]
        public async Task GetOwnAuditLogs_ReturnsOnlyUserLogs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            await CreateTestAuditLog(userId, "my_action1");
            await CreateTestAuditLog(userId, "my_action2");
            await CreateTestAuditLog(otherUserId, "other_action");

            SetupRegularUser(userId);

            // Act
            var result = await _controller.GetOwnAuditLogs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var logs = Assert.IsAssignableFrom<System.Collections.Generic.List<AuditLogDto>>(okResult.Value);
            Assert.Equal(2, logs.Count);
            Assert.All(logs, log => Assert.Equal(userId, log.UserId));
        }

        // ========================================
        // Get Audit Log By ID Tests
        // ========================================

        [Fact]
        public async Task GetAuditLogById_ExistingLog_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var auditLog = await CreateTestAuditLog(userId, "test_action");

            SetupAdminUser();

            // Act
            var result = await _controller.GetAuditLogById(auditLog.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var auditLogDto = Assert.IsType<AuditLogDto>(okResult.Value);
            Assert.Equal(auditLog.Id, auditLogDto.Id);
            Assert.Equal("test_action", auditLogDto.Action);
        }

        [Fact]
        public async Task GetAuditLogById_NonExistingLog_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            SetupAdminUser();

            // Act
            var result = await _controller.GetAuditLogById(nonExistentId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ========================================
        // Cleanup Tests
        // ========================================

        [Fact]
        public async Task CleanupOldLogs_DeletesOldNonCriticalLogs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldDate = DateTime.UtcNow.AddDays(-800); // Older than 730 days

            // Create old non-critical log (should be deleted)
            await CreateTestAuditLog(userId, "old_action", severity: "Info", timestamp: oldDate);

            // Create old critical log (should be kept)
            await CreateTestAuditLog(userId, "old_critical", severity: "Critical", timestamp: oldDate);

            // Create recent log (should be kept)
            await CreateTestAuditLog(userId, "recent_action", severity: "Info", timestamp: DateTime.UtcNow);

            SetupAdminUser();

            // Act
            var result = await _controller.CleanupOldLogs(olderThanDays: 730);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var remainingLogs = await _context.AuditLogs.ToListAsync();
            Assert.Equal(2, remainingLogs.Count); // Critical + Recent
            Assert.Contains(remainingLogs, l => l.Severity == "Critical");
            Assert.Contains(remainingLogs, l => l.Action == "recent_action");
        }

        [Fact]
        public async Task CleanupOldLogs_CustomRetentionPeriod_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            await CreateTestAuditLog(userId, "action1", timestamp: DateTime.UtcNow.AddDays(-100));
            await CreateTestAuditLog(userId, "action2", timestamp: DateTime.UtcNow.AddDays(-40));

            SetupAdminUser();

            // Act - Delete logs older than 50 days
            var result = await _controller.CleanupOldLogs(olderThanDays: 50);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var remainingLogs = await _context.AuditLogs.ToListAsync();
            Assert.Single(remainingLogs); // Only the 40-day-old log remains
            Assert.Equal("action2", remainingLogs[0].Action);
        }

        // ========================================
        // Helper Methods
        // ========================================

        private async Task<AuditLog> CreateTestAuditLog(
            Guid userId, 
            string action, 
            string severity = "Info", 
            DateTime? timestamp = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                Resource = "TestService",
                Details = "{}",
                IpAddress = "127.0.0.1",
                Timestamp = timestamp ?? DateTime.UtcNow,
                Severity = severity
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            return auditLog;
        }

        private void SetupAdminUser()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private void SetupRegularUser(Guid userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
