using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Security;

namespace Prescription.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options, IColumnEncryptor columnEncryptor)
    : DbContext(options), IUnitOfWork
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

            builder.Property(o => o.BasicInsurance).HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.SupplementaryInsurance).HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.ThirdPartyPhoneNumber).HasMaxLength(20);

            // Same column-level encryption as the customer's own national code in Identity.
            builder.Property(o => o.ThirdPartyNationalCode)
                .HasConversion(
                    plain => plain == null ? null : columnEncryptor.Encrypt(plain),
                    cipher => cipher == null ? null : columnEncryptor.Decrypt(cipher))
                .HasMaxLength(500);

            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.Status);

            // Postgres native array column (uuid[]) backed by the private _labTestIds field,
            // since LabTestIds is exposed publicly as a read-only collection.
            builder.PrimitiveCollection(o => o.LabTestIds)
                .HasField("_labTestIds")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("lab_test_ids");

            builder.Property(o => o.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });

        base.OnModelCreating(modelBuilder);
    }
}
