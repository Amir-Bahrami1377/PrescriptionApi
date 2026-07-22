using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<LabTest> LabTests => Set<LabTest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<LabTest>(builder =>
        {
            builder.ToTable("lab_tests");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
            builder.Property(t => t.Description).HasMaxLength(2000);

            builder.Property(t => t.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });

        base.OnModelCreating(modelBuilder);
    }
}
