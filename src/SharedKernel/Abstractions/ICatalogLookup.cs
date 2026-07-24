namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Read-only cross-module contract so Orders/Consultation can confirm lab tests exist and are
/// active without taking a project reference on the Catalog module — implemented by
/// Prescription.Modules.Catalog. Pricing lives with the reviewing doctor (see IIdentityLookup),
/// not with the catalog entry.
/// </summary>
public interface ICatalogLookup
{
    Task<IReadOnlyList<LabTestSnapshot>> GetActiveTestsAsync(IEnumerable<Guid> labTestIds, CancellationToken cancellationToken = default);
}

public sealed record LabTestSnapshot(Guid Id, string Name);
