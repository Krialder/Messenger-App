// ========================================
// AUDIT CONTROLLER - Sprint 11: Audit Log Service
// Status: 🔶 API Implementation
// Purpose: DSGVO Art. 30 - Verzeichnis von Verarbeitungstätigkeiten
// ========================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuditLogService.Data;
using AuditLogService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuditLogService.Controllers
{
    /// <summary>
    /// Controller for audit log management (DSGVO Art. 30 compliance)
    /// Provides secure audit trail for all system actions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly AuditDbContext _context;
        private readonly ILogger<AuditController> _logger;

        public AuditController(AuditDbContext context, ILogger<AuditController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get audit logs with filtering (Admin-only)
        /// GET /api/audit/logs?userId={guid}&action={string}&page=1&pageSize=50
        /// </summary>
        [HttpGet("logs")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] string? severity = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action.Contains(action));

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(a => a.Severity == severity);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    Resource = a.Resource,
                    Details = a.Details,
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp,
                    Severity = a.Severity
                })
                .ToListAsync();

            return Ok(new AuditLogResponse
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// Get own audit logs (User can see their own activity)
        /// GET /api/audit/me
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOwnAuditLogs(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized();
            }

            var query = _context.AuditLogs.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    Resource = a.Resource,
                    Details = a.Details,
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp,
                    Severity = a.Severity
                })
                .ToListAsync();

            return Ok(logs);
        }

        /// <summary>
        /// Create audit log entry (Internal API - used by other services)
        /// POST /api/audit/log
        /// </summary>
        [HttpPost("log")]
        [AllowAnonymous] // Internal services can call this
        [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = request.Action,
                Resource = request.Resource,
                Details = request.Details ?? string.Empty,
                IpAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Timestamp = DateTime.UtcNow,
                Severity = request.Severity ?? "Info"
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {Action} by user {UserId}", auditLog.Action, auditLog.UserId);

            return CreatedAtAction(nameof(GetAuditLogById), new { id = auditLog.Id }, new AuditLogDto
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                Resource = auditLog.Resource,
                Details = auditLog.Details,
                IpAddress = auditLog.IpAddress,
                Timestamp = auditLog.Timestamp,
                Severity = auditLog.Severity
            });
        }

        /// <summary>
        /// Get specific audit log by ID (Admin-only)
        /// GET /api/audit/logs/{id}
        /// </summary>
        [HttpGet("logs/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuditLogById(Guid id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);

            if (auditLog == null)
                return NotFound();

            return Ok(new AuditLogDto
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                Resource = auditLog.Resource,
                Details = auditLog.Details,
                IpAddress = auditLog.IpAddress,
                Timestamp = auditLog.Timestamp,
                Severity = auditLog.Severity
            });
        }

        /// <summary>
        /// Delete old audit logs (DSGVO compliance - max 2 years retention)
        /// DELETE /api/audit/cleanup?olderThanDays=730
        /// </summary>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CleanupOldLogs([FromQuery] int olderThanDays = 730)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

            var logsToDelete = await _context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate && a.Severity != "Critical")
                .ToListAsync();

            _context.AuditLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} audit logs older than {Date}", logsToDelete.Count, cutoffDate);

            return Ok(new { DeletedCount = logsToDelete.Count, CutoffDate = cutoffDate });
        }
    }

    // ========================================
    // DTOs
    // ========================================

    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    public class CreateAuditLogRequest
    {
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? Severity { get; set; }
    }

    public class AuditLogResponse
    {
        public List<AuditLogDto> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
