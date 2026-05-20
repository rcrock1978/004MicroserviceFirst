using IdentityService.Application;
using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;

namespace IdentityService.Application.Tests;

public class TestIdentityDbContext : DbContext, IIdentityDbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public TestIdentityDbContext(DbContextOptions<TestIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.TenantId);
            entity.Property(e => e.ExternalId);
            entity.Property(e => e.Email);
            entity.Property(e => e.DisplayName);
            entity.Ignore(e => e.Roles);
            entity.Ignore(e => e.Claims);
        });
    }
}
