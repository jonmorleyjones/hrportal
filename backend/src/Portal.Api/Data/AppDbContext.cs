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
    public DbSet<OnboardingSurvey> OnboardingSurveys => Set<OnboardingSurvey>();
    public DbSet<OnboardingSurveyVersion> OnboardingSurveyVersions => Set<OnboardingSurveyVersion>();
    public DbSet<OnboardingResponse> OnboardingResponses => Set<OnboardingResponse>();

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

        // OnboardingSurvey configuration
        modelBuilder.Entity<OnboardingSurvey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ActiveVersion)
                .WithOne()
                .HasForeignKey<OnboardingSurvey>(e => e.ActiveVersionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // OnboardingSurveyVersion configuration
        modelBuilder.Entity<OnboardingSurveyVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SurveyId, e.VersionNumber }).IsUnique();

            entity.HasOne(e => e.Survey)
                .WithMany(s => s.Versions)
                .HasForeignKey(e => e.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OnboardingResponse configuration
        modelBuilder.Entity<OnboardingResponse>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.SurveyVersion)
                .WithMany(v => v.Responses)
                .HasForeignKey(e => e.SurveyVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Global query filter for multi-tenancy
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<Invitation>().HasQueryFilter(i => i.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(a => a.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(r => r.User.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<OnboardingSurvey>().HasQueryFilter(s => s.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<OnboardingSurveyVersion>().HasQueryFilter(v => v.Survey.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
        modelBuilder.Entity<OnboardingResponse>().HasQueryFilter(r => r.SurveyVersion.Survey.TenantId == _tenantContext.TenantId || _tenantContext.TenantId == Guid.Empty);
    }
}
