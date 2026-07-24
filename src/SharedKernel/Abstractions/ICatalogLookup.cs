namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Read-only cross-module contract so Orders/Consultation can confirm a lab test exists and is
/// active without taking a project reference on the Catalog module — implemented by
/// Prescription.Modules.Catalog. Pricing lives with the reviewing doctor (see IIdentityLookup),
/// not with the catalog entry.
/// </summary>
public interface ICatalogLookup
{
    Task<LabTestSnapshot?> GetActiveTestAsync(Guid labTestId, CancellationToken cancellationToken = default);
}

public sealed record LabTestSnapshot(Guid Id, string Name);
