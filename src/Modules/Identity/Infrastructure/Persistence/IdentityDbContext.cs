using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Security;

namespace Prescription.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options, IColumnEncryptor columnEncryptor)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.HasIndex(u => u.PhoneNumber).IsUnique();
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            builder.Property(u => u.Gender).HasConversion<string>().HasMaxLength(10);
            builder.Property(u => u.FullName).HasMaxLength(200);

            builder.Property(u => u.NationalCode)
                .HasConversion(
                    plain => plain == null ? null : columnEncryptor.Encrypt(plain),
                    cipher => cipher == null ? null : columnEncryptor.Decrypt(cipher))
                .HasMaxLength(500);

            // Postgres system column "xmin" as the optimistic-concurrency token backing AuditableEntity.RowVersion.
            builder.Property(u => u.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });

        base.OnModelCreating(modelBuilder);
    }
}
