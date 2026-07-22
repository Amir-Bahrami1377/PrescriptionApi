using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");

        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.CustomerNote).HasMaxLength(2000);
            builder.Property(o => o.CustomerUploadedFileKey).HasMaxLength(500);
            builder.Property(o => o.RejectionReason).HasMaxLength(1000);
            builder.Property(o => o.PrescriptionReferenceNumber).HasMaxLength(200);
            builder.Property(o => o.PaymentAuthority).HasMaxLength(100);
            builder.Property(o => o.PaymentReferenceId).HasMaxLength(100);
            builder.Property(o => o.ResultFileKey).HasMaxLength(500);

            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.Status);

            builder.Property(o => o.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });

        base.OnModelCreating(modelBuilder);
    }
}
