using Microsoft.EntityFrameworkCore;
using AuditLogService.Data.Entities;

namespace AuditLogService.Data
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs", "audit");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.Action)
                    .HasColumnName("action")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Resource)
                    .HasColumnName("resource")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Details)
                    .HasColumnName("details")
                    .HasColumnType("jsonb");

                entity.Property(e => e.IpAddress)
                    .HasColumnName("ip_address")
                    .HasMaxLength(45);

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.Severity)
                    .HasColumnName("severity")
                    .HasMaxLength(20)
                    .HasDefaultValue("Info");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("idx_audit_user");

                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("idx_audit_timestamp");

                entity.HasIndex(e => e.Action)
                    .HasDatabaseName("idx_audit_action");
            });
        }
    }
}
