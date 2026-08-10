using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Infrastructure.Persistence;

namespace Prescription.Modules.Catalog.Features.ListTests;

public sealed class ListTestsHandler(CatalogDbContext dbContext) : IRequestHandler<ListTestsQuery, IReadOnlyList<LabTestDto>>
{
    public async Task<IReadOnlyList<LabTestDto>> Handle(ListTestsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.LabTests.AsNoTracking().Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => EF.Functions.ILike(t.Name, $"%{request.Search}%"));
        }

        return await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .Select(t => new LabTestDto(t.Id, t.Name, t.Description))
            .ToListAsync(cancellationToken);
    }
}
