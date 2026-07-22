using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Infrastructure;

internal sealed class CatalogLookup(CatalogDbContext dbContext) : ICatalogLookup
{
    public async Task<LabTestSnapshot?> GetActiveTestAsync(Guid labTestId, CancellationToken cancellationToken = default)
    {
        return await dbContext.LabTests
            .AsNoTracking()
            .Where(t => t.Id == labTestId && t.IsActive)
            .Select(t => new LabTestSnapshot(t.Id, t.Name, t.PriceInRials))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
