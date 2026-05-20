using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain;

namespace NotificationService.Infrastructure.EntityConfigurations;

public sealed class DeliveryLogConfiguration : IEntityTypeConfiguration<DeliveryLog>
{
    public void Configure(EntityTypeBuilder<DeliveryLog> builder)
    {
        builder.ToTable("delivery_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Recipient).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Channel).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(4000);
        builder.Property(x => x.SentAt).IsRequired();
        builder.HasIndex(x => x.SentAt);
    }
}
