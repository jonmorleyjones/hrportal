using Microsoft.EntityFrameworkCore;
using Portal.Api.Models;
using Portal.Api.Services;

namespace Portal.Api.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<RequestTypeVersion> RequestTypeVersions => Set<RequestTypeVersion>();
    public DbSet<RequestResponse> RequestResponses => Set<RequestResponse>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Slug).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SubscriptionTier).HasMaxLength(20);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Invitation configuration
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Token).HasMaxLength(100);
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Invitations)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.AuditLogs)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RequestType configuration
        modelBuilder.Entity<RequestType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50).HasDefaultValue("clipboard-list");

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ActiveVersion)
                .WithOne()
                .HasForeignKey<RequestType>(e => e.ActiveVersionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RequestTypeVersion configuration
        modelBuilder.Entity<RequestTypeVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RequestTypeId, e.VersionNumber }).IsUnique();

            entity.HasOne(e => e.RequestType)
                .WithMany(s => s.Versions)
                .HasForeignKey(e => e.RequestTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RequestResponse configuration
        modelBuilder.Entity<RequestResponse>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.RequestTypeVersion)
                .WithMany(v => v.Responses)
                .HasForeignKey(e => e.RequestTypeVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UploadedFile configuration
        modelBuilder.Entity<UploadedFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionName).HasMaxLength(200);
            entity.Property(e => e.OriginalFileName).HasMaxLength(500);
            entity.Property(e => e.StoredFileName).HasMaxLength(100);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.StoragePath).HasMaxLength(500);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RequestResponse)
                .WithMany()
                .HasForeignKey(e => e.RequestResponseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.UploadedAt, e.RequestResponseId })
                .HasFilter("\"RequestResponseId\" IS NULL");
        });

        // Global query filter for multi-tenancy
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<Invitation>().HasQueryFilter(i => i.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(a => a.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(r => r.User.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<RequestType>().HasQueryFilter(s => s.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<RequestTypeVersion>().HasQueryFilter(v => v.RequestType.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<RequestResponse>().HasQueryFilter(r => r.RequestTypeVersion.RequestType.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<UploadedFile>().HasQueryFilter(f => f.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
    }
}
