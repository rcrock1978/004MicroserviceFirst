using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using PaymentService.Domain;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(v => v.Value, v => new TenantId(v)));

        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ProviderReference)
            .HasMaxLength(200);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.ProcessedAt);
        builder.Property(p => p.CreatedAt);
        builder.Property(p => p.UpdatedAt);

        builder.HasIndex(p => p.OrderId).IsUnique();
    }
}
