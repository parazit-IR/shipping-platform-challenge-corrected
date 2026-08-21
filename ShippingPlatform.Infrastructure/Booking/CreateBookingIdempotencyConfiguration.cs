using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShippingPlatform.Infrastructure.Booking;

internal sealed class CreateBookingIdempotencyConfiguration
    : IEntityTypeConfiguration<CreateBookingIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<CreateBookingIdempotencyRecord> builder)
    {
        builder.ToTable(
            CreateBookingIdempotencySchema.TableName,
            CreateBookingIdempotencySchema.SchemaName);

        builder.HasKey(x => x.IdempotencyKey)
            .HasName(CreateBookingIdempotencySchema.PrimaryKeyName);

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(200);

        builder.Property(x => x.RequestFingerprint)
            .HasColumnName("request_fingerprint")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasColumnName("state")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id");

        builder.Property(x => x.BookingStatus)
            .HasColumnName("booking_status")
            .HasMaxLength(64);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");
    }
}
