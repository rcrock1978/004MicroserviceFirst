using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using CustomerService.Domain;

namespace CustomerService.Infrastructure.EntityConfigurations;

public sealed class CustomerOrderHistoryConfiguration : IEntityTypeConfiguration<CustomerOrderHistory>
{
    public void Configure(EntityTypeBuilder<CustomerOrderHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(
                v => v.Value,
                v => new TenantId(v)));

        builder.Property(h => h.OrderId).IsRequired();
        builder.Property(h => h.CustomerId).IsRequired();
        builder.Property(h => h.Status).HasMaxLength(50).IsRequired();
        builder.Property(h => h.OrderDate);
        builder.Property(h => h.CreatedAt);
        builder.Property(h => h.UpdatedAt);

        builder.HasIndex(h => new { h.TenantId, h.CustomerId });
        builder.HasIndex(h => h.OrderId).IsUnique();
    }
}
