using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(v => v.Value, v => new TenantId(v)));

        builder.Property(s => s.ProductId).IsRequired();
        builder.HasIndex(s => s.ProductId).IsUnique();

        builder.Property(s => s.QuantityAvailable);
        builder.Property(s => s.QuantityReserved);

        builder.Property(s => s.CreatedAt);
        builder.Property(s => s.UpdatedAt);
    }
}
