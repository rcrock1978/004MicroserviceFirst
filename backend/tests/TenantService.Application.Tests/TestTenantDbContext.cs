using SaaSCommon.Domain;
using Microsoft.EntityFrameworkCore;
using TenantService.Application;
using TenantService.Domain;

namespace TenantService.Application.Tests;

public class TestTenantDbContext : DbContext, ITenantDbContext
{
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public TestTenantDbContext(DbContextOptions<TestTenantDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.Name);
            entity.Property(e => e.Slug);
            entity.Property(e => e.Status);
            entity.Property(e => e.TenantId);
            entity.Ignore(e => e.FeatureFlags);
            entity.Ignore(e => e.Configuration);
        });
    }
}
