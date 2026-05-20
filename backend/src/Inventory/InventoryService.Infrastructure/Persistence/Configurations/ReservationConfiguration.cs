using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(v => v.Value, v => new TenantId(v)));

        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.Quantity);
        builder.Property(r => r.ExpiresAt);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.StockItemId).IsRequired();
        builder.HasIndex(r => r.StockItemId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => new { r.Status, r.ExpiresAt });

        builder.Property(r => r.CreatedAt);
        builder.Property(r => r.UpdatedAt);
    }
}
