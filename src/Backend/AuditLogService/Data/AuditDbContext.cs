// ========================================
// PSEUDO-CODE - Sprint 11: Audit Log Database Context
// ========================================

using Microsoft.EntityFrameworkCore;

namespace SecureMessenger.AuditLogService.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }
    
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6
        });
    }
}

// ========================================
// ENTITIES
// ========================================

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; } // JSON
    public bool IsCritical { get; set; } // Keep longer for critical events
}
