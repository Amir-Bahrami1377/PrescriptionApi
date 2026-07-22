namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Read-only cross-module contract so Orders/Consultation can resolve catalog data (price, name)
/// without taking a project reference on the Catalog module — implemented by Prescription.Modules.Catalog.
/// </summary>
public interface ICatalogLookup
{
    Task<LabTestSnapshot?> GetActiveTestAsync(Guid labTestId, CancellationToken cancellationToken = default);
}

public sealed record LabTestSnapshot(Guid Id, string Name, long PriceInRials);
