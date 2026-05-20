using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using OrderService.Domain;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(v => v.Value, v => new TenantId(v)));

        builder.Property(o => o.CustomerId).IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.PaymentProviderReference)
            .HasMaxLength(200);

        builder.Property(o => o.CreatedAt);
        builder.Property(o => o.UpdatedAt);

        builder.OwnsMany(o => o.Items, item =>
        {
            item.ToTable("order_items");
            item.HasKey("Id");
            item.WithOwner().HasForeignKey("OrderId");
            item.Property(i => i.ProductId).IsRequired();
            item.Property(i => i.Quantity);
            item.Property(i => i.UnitPrice).HasPrecision(18, 2);
            item.Ignore(i => i.LineTotal);
        });
    }
}
