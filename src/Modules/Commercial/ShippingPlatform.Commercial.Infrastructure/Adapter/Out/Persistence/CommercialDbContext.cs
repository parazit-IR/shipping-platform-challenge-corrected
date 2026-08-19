using Microsoft.EntityFrameworkCore;

namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;

public sealed class CommercialDbContext(DbContextOptions<CommercialDbContext> options) : DbContext(options)
{
    public DbSet<CustomerRecord> Customers => Set<CustomerRecord>();
    public DbSet<AgreementRecord> Agreements => Set<AgreementRecord>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerRecord>(entity =>
        {
            entity.ToTable("customers", "commercial");
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.CustomerId).HasColumnName("customer_id").HasMaxLength(64);
        });

        modelBuilder.Entity<AgreementRecord>(entity =>
        {
            entity.ToTable("agreements", "commercial");
            entity.HasKey(x => x.AgreementId);
            entity.Property(x => x.AgreementId).HasColumnName("agreement_id").HasMaxLength(64);
            entity.Property(x => x.CustomerId)
                .HasColumnName("customer_id").HasMaxLength(64).IsRequired();

            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).IsRequired();

            entity.HasOne<CustomerRecord>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}