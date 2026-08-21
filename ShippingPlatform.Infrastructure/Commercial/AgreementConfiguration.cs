using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShippingPlatform.Domain.Commercial.ValueObject;

using AgreementAggregate = ShippingPlatform.Domain.Commercial.Entity.Agreement;

namespace ShippingPlatform.Infrastructure.Commercial;

public sealed class AgreementConfiguration
    : IEntityTypeConfiguration<AgreementAggregate>
{
    public void Configure(
        EntityTypeBuilder<AgreementAggregate> builder)
    {
        builder.ToTable("agreements", "commercial");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => AgreementId.Create(value))
            .HasColumnName("agreement_id")
            .HasMaxLength(64);

        builder.Property(x => x.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value))
            .HasColumnName("customer_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();
        
        builder.HasOne<CustomerRecord>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .HasPrincipalKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}