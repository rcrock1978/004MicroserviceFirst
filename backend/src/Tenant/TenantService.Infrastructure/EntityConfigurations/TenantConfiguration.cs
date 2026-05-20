using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Infrastructure.EntityConfigurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(
                v => v.Value,
                v => new TenantId(v)));

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.OwnsOne(t => t.Configuration, config =>
        {
            config.Property(c => c.TimeZone)
                .HasMaxLength(100)
                .HasColumnName("time_zone");

            config.Property(c => c.DefaultLanguage)
                .HasMaxLength(10)
                .HasColumnName("default_language");

            config.Property(c => c.MaxUsers)
                .HasColumnName("max_users");

            config.Property(c => c.Settings)
                .HasColumnType("jsonb")
                .HasColumnName("settings");
        });

        builder.OwnsMany(t => t.FeatureFlags, feature =>
        {
            feature.ToTable("tenant_feature_flags");
            feature.HasKey("Id");
            feature.WithOwner().HasForeignKey("TenantId");
            feature.Property(f => f.Key).IsRequired().HasMaxLength(100);
            feature.Property(f => f.Enabled);
            feature.Property(f => f.Description).HasMaxLength(500);
        });

        builder.Property(t => t.CreatedAt);
        builder.Property(t => t.UpdatedAt);
    }
}
