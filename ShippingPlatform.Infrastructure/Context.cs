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
    internal DbSet<Booking.CreateBookingIdempotencyRecord> CreateBookingIdempotencyRecords =>
        Set<Booking.CreateBookingIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Booking.BookingConfiguration());
        modelBuilder.ApplyConfiguration(new Booking.CreateBookingIdempotencyConfiguration());
        modelBuilder.ApplyConfiguration(new Commercial.AgreementConfiguration());
        modelBuilder.ApplyConfiguration(new Commercial.CustomerConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyBookingOptimisticConcurrencyVersioning();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyBookingOptimisticConcurrencyVersioning();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Centralizes optimistic-concurrency version advancement for the Booking aggregate so
    // every persistence path through this Context (WriteUnitOfWork, executors calling
    // SaveChangesAsync directly, sync SaveChanges, etc.) behaves consistently.
    //
    // The next version is always computed from EF's tracked *original* value rather than
    // the current value. That keeps the predicate EF generates for the UPDATE statement
    // correct (original version) and makes this idempotent across retries: if a prior
    // SaveChanges call failed (e.g. with DbUpdateConcurrencyException) and the same tracked
    // entity is saved again without further modification, the original value hasn't changed,
    // so the computed next version stays the same instead of climbing higher each attempt.
    //
    // Only entries whose EF state is Modified are touched. Newly inserted Bookings already
    // start at Version = 1 from the domain entity; unchanged entities are left alone entirely.
    private void ApplyBookingOptimisticConcurrencyVersioning()
    {
        foreach (var entry in ChangeTracker.Entries<BookingAggregate>())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            var versionProperty = entry.Property(nameof(BookingAggregate.Version));
            var originalVersion = (long)versionProperty.OriginalValue!;

            versionProperty.CurrentValue = originalVersion + 1;
        }
    }
}
