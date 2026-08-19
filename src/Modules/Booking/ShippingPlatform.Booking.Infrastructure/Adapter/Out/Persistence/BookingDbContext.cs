using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence.Entity;

namespace ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext>  options): DbContext(options)
{
    public DbSet<BookingRecord> Bookings => Set<BookingRecord>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingRecord>(entity =>
        {
            entity.ToTable("bookings", "booking");

            entity.HasKey(x => x.BookingId);

            entity.Property(x => x.BookingId)
                .HasColumnName("booking_id");

            entity.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(x => x.AgreementId)
                .HasColumnName("agreement_id")
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(x => x.Origin)
                .HasColumnName("origin")
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(x => x.Destination)
                .HasColumnName("destination")
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(x => x.VoyageId)
                .HasColumnName("voyage_id")
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
        });
    }
    
}