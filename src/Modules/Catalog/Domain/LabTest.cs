using Prescription.SharedKernel.Entities;

namespace Prescription.Modules.Catalog.Domain;

public sealed class LabTest : AuditableEntity
{
    private LabTest() { }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>Position in the catalogue listing. Seeded tests carry the curated order they are
    /// authored in; anything an admin adds later is appended after them. Without this the listing
    /// falls back to sorting by name, which scatters the intended grouping.</summary>
    public int DisplayOrder { get; private set; }

    public static LabTest Create(string name, string? description, int displayOrder)
    {
        return new LabTest
        {
            Name = name,
            Description = description,
            IsActive = true,
            DisplayOrder = displayOrder,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void SetDisplayOrder(int displayOrder)
    {
        if (DisplayOrder == displayOrder)
        {
            return;
        }

        DisplayOrder = displayOrder;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft delete: the row stays so historical orders keep resolving the test they were placed
    /// for (Order.LabTestIds holds bare Guids with no FK), while both the catalog listing and the
    /// cross-module lookup already filter on IsActive, so it disappears from every caller's view.</summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
