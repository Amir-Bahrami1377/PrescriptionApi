namespace Prescription.SharedKernel.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    /// <summary>EF Core Optimistic Concurrency token — required for the Orders state machine to avoid race conditions.</summary>
    public uint RowVersion { get; set; }
}
