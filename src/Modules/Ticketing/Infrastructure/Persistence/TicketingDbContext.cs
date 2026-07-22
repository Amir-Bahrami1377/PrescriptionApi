using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Infrastructure.Persistence;

public sealed class TicketingDbContext(DbContextOptions<TicketingDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ticketing");

        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.ToTable("tickets");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Subject).HasMaxLength(300).IsRequired();
            builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(t => t.CustomerId);

            builder.Property(t => t.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.Metadata.FindNavigation(nameof(Ticket.Messages))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(t => t.Messages, messageBuilder =>
            {
                messageBuilder.ToTable("ticket_messages");
                messageBuilder.WithOwner().HasForeignKey(m => m.TicketId);
                messageBuilder.HasKey(m => m.Id);
                messageBuilder.Property(m => m.SenderRole).HasMaxLength(20).IsRequired();
                messageBuilder.Property(m => m.Body).HasMaxLength(4000).IsRequired();
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}
