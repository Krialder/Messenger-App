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
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 50;

        public AuditController(AuditDbContext context, ILogger<AuditController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get audit logs with filtering (Admin-only)
        /// </summary>
        [HttpGet("logs")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] string? severity = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > MaxPageSize) pageSize = DefaultPageSize;

                var query = _context.AuditLogs.AsNoTracking().AsQueryable();

                if (userId.HasValue)
                    query = query.Where(a => a.UserId == userId.Value);

                if (!string.IsNullOrWhiteSpace(action))
                    query = query.Where(a => a.Action.Contains(action));

                if (!string.IsNullOrWhiteSpace(severity))
                    query = query.Where(a => a.Severity == severity);

                if (startDate.HasValue)
                    query = query.Where(a => a.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.Timestamp <= endDate.Value);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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

                _logger.LogInformation("Admin retrieved {Count} audit logs (page {Page}/{TotalPages})", 
                    logs.Count, page, totalPages);

                return Ok(new AuditLogResponse
                {
                    Logs = logs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, new { error = "Failed to retrieve audit logs" });
            }
        }

        /// <summary>
        /// Get own audit logs (User can see their own activity)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOwnAuditLogs(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > MaxPageSize) pageSize = DefaultPageSize;

                var userId = GetUserId();

                var query = _context.AuditLogs.AsNoTracking().Where(a => a.UserId == userId);

                if (startDate.HasValue)
                    query = query.Where(a => a.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.Timestamp <= endDate.Value);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user audit logs");
                return StatusCode(500, new { error = "Failed to retrieve audit logs" });
            }
        }

        /// <summary>
        /// Create audit log entry (Internal API - used by other services)
        /// </summary>
        [HttpPost("log")]
        [AllowAnonymous] // Internal services can call this
        [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Action))
                {
                    return BadRequest(new { error = "Action is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Resource))
                {
                    return BadRequest(new { error = "Resource is required" });
                }

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Action = request.Action.Trim(),
                    Resource = request.Resource.Trim(),
                    Details = request.Details?.Trim() ?? string.Empty,
                    IpAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    Severity = ValidateSeverity(request.Severity)
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit log created: {Action} on {Resource} by user {UserId} (Severity: {Severity})", 
                    auditLog.Action, auditLog.Resource, auditLog.UserId, auditLog.Severity);

                var dto = MapToDto(auditLog);
                return CreatedAtAction(nameof(GetAuditLogById), new { id = auditLog.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log");
                return StatusCode(500, new { error = "Failed to create audit log" });
            }
        }

        /// <summary>
        /// Get specific audit log by ID (Admin-only)
        /// </summary>
        [HttpGet("logs/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAuditLogById(Guid id)
        {
            try
            {
                var auditLog = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                if (auditLog == null)
                {
                    return NotFound(new { error = "Audit log not found" });
                }

                return Ok(MapToDto(auditLog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {Id}", id);
                return StatusCode(500, new { error = "Failed to retrieve audit log" });
            }
        }

        /// <summary>
        /// Get audit log statistics (Admin-only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AuditStatisticsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.AuditLogs.AsNoTracking().AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(a => a.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.Timestamp <= endDate.Value);

                var stats = new AuditStatisticsDto
                {
                    TotalLogs = await query.CountAsync(),
                    LogsBySeverity = await query.GroupBy(a => a.Severity)
                        .Select(g => new SeverityCount { Severity = g.Key, Count = g.Count() })
                        .ToListAsync(),
                    LogsByAction = await query.GroupBy(a => a.Action)
                        .Select(g => new ActionCount { Action = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .ToListAsync(),
                    StartDate = startDate,
                    EndDate = endDate
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit statistics");
                return StatusCode(500, new { error = "Failed to retrieve statistics" });
            }
        }

        /// <summary>
        /// Delete old audit logs (DSGVO compliance - max 2 years retention)
        /// </summary>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CleanupOldLogs([FromQuery] int olderThanDays = 730)
        {
            try
            {
                if (olderThanDays < 90)
                {
                    return BadRequest(new { error = "Cannot delete logs less than 90 days old" });
                }

                var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

                var logsToDelete = await _context.AuditLogs
                    .Where(a => a.Timestamp < cutoffDate && a.Severity != "Critical")
                    .ToListAsync();

                _context.AuditLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Deleted {Count} audit logs older than {Date} ({Days} days)", 
                    logsToDelete.Count, cutoffDate, olderThanDays);

                return Ok(new
                {
                    deletedCount = logsToDelete.Count,
                    cutoffDate = cutoffDate,
                    olderThanDays = olderThanDays
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up audit logs");
                return StatusCode(500, new { error = "Failed to cleanup audit logs" });
            }
        }

        #region Helper Methods

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                _logger.LogError("Invalid or missing user ID claim");
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        private static string ValidateSeverity(string? severity)
        {
            var validSeverities = new[] { "Info", "Warning", "Error", "Critical" };
            return validSeverities.Contains(severity) ? severity : "Info";
        }

        private static AuditLogDto MapToDto(AuditLog auditLog)
        {
            return new AuditLogDto
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                Resource = auditLog.Resource,
                Details = auditLog.Details,
                IpAddress = auditLog.IpAddress,
                Timestamp = auditLog.Timestamp,
                Severity = auditLog.Severity
            };
        }

        #endregion
    }

    #region DTOs

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
        public int TotalPages { get; set; }
    }

    public class AuditStatisticsDto
    {
        public int TotalLogs { get; set; }
        public List<SeverityCount> LogsBySeverity { get; set; } = new();
        public List<ActionCount> LogsByAction { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SeverityCount
    {
        public string Severity { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ActionCount
    {
        public string Action { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    #endregion
}
