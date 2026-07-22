using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Consultation.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Consultation.Infrastructure.Persistence;

public sealed class ConsultationDbContext(DbContextOptions<ConsultationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<ConsultationRequest> ConsultationRequests => Set<ConsultationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("consultation");

        modelBuilder.Entity<ConsultationRequest>(builder =>
        {
            builder.ToTable("consultation_requests");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ImageFileKey).HasMaxLength(500).IsRequired();
            builder.Property(c => c.CustomerNote).HasMaxLength(2000);
            builder.Property(c => c.DoctorOpinion).HasMaxLength(4000);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(c => c.CustomerId);
            builder.HasIndex(c => c.Status);

            builder.Property(c => c.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });

        base.OnModelCreating(modelBuilder);
    }
}
