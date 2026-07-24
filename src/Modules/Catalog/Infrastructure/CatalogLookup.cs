using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Infrastructure;

internal sealed class CatalogLookup(CatalogDbContext dbContext) : ICatalogLookup
{
    public async Task<IReadOnlyList<LabTestSnapshot>> GetActiveTestsAsync(IEnumerable<Guid> labTestIds, CancellationToken cancellationToken = default)
    {
        var ids = labTestIds.Distinct().ToList();

        return await dbContext.LabTests
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id) && t.IsActive)
            .Select(t => new LabTestSnapshot(t.Id, t.Name))
            .ToListAsync(cancellationToken);
    }
}
