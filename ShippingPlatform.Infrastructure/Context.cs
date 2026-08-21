using Microsoft.EntityFrameworkCore;
using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;
using AgreementAggregate = ShippingPlatform.Domain.Commercial.Entity.Agreement;

namespace ShippingPlatform.Infrastructure;

public sealed class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public DbSet<BookingAggregate> Bookings => Set<BookingAggregate>();
    public DbSet<AgreementAggregate> Agreements => Set<AgreementAggregate>();
    internal DbSet<Commercial.CustomerRecord> Customers => Set<Commercial.CustomerRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Booking.BookingConfiguration());
        modelBuilder.ApplyConfiguration(new Commercial.AgreementConfiguration());
        modelBuilder.ApplyConfiguration(new Commercial.CustomerConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}