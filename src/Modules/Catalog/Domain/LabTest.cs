using Prescription.SharedKernel.Entities;

namespace Prescription.Modules.Catalog.Domain;

public sealed class LabTest : AuditableEntity
{
    private LabTest() { }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static LabTest Create(string name, string? description)
    {
        return new LabTest
        {
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
