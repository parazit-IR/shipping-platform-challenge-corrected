using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShippingPlatform.Domain.Booking.ValueObject;
using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Infrastructure.Booking;

public sealed class BookingConfiguration : IEntityTypeConfiguration<BookingAggregate>
{
    public void Configure(EntityTypeBuilder<BookingAggregate> builder)
    {
        builder.ToTable("bookings", "booking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(id => id.Value, value => new BookingId(value))
            .HasColumnName("booking_id");

        builder.Property(x => x.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.Create(value))
            .HasColumnName("customer_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.AgreementId)
            .HasConversion(
                id => id.Value,
                value => AgreementId.Create(value))
            .HasColumnName("agreement_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Origin)
            .HasConversion(
                origin => origin.Value,
                value => Origin.Create(value))
            .HasColumnName("origin")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Destination)
            .HasConversion(
                destination => destination.Value,
                value => Destination.Create(value))
            .HasColumnName("destination")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.VoyageId)
            .HasConversion(
                id => id.Value,
                value => VoyageId.Create(value))
            .HasColumnName("voyage_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasColumnType("bigint")
            .HasDefaultValue(1L)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Ignore("_containerRequests");

        builder.Ignore(x => x.DomainEvents);
    }
}