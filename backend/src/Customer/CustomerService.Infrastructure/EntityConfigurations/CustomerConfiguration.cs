using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaSCommon.Domain;
using CustomerService.Domain;

namespace CustomerService.Infrastructure.EntityConfigurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasConversion(new ValueConverter<TenantId, Guid>(
                v => v.Value,
                v => new TenantId(v)));

        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.CreatedAt);
        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => new { c.TenantId, c.Email }).IsUnique();
    }
}
