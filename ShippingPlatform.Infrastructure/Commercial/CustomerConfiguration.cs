using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShippingPlatform.Domain.Commercial.ValueObject;

namespace ShippingPlatform.Infrastructure.Commercial;

internal sealed class CustomerConfiguration
    : IEntityTypeConfiguration<CustomerRecord>
{
    public void Configure(
        EntityTypeBuilder<CustomerRecord> builder)
    {
        builder.ToTable("customers", "commercial");

        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value))
            .HasColumnName("customer_id")
            .HasMaxLength(64);
    }
}