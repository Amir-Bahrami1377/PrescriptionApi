using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Domain;
using Prescription.Modules.Catalog.Infrastructure.Persistence;

namespace Prescription.Modules.Catalog.Features.CreateTest;

public sealed class CreateTestHandler(CatalogDbContext dbContext) : IRequestHandler<CreateTestCommand, CreateTestResponse>
{
    public async Task<CreateTestResponse> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        // Appended to the end of the catalogue. Cast to int? so an empty table yields null rather
        // than throwing, which Max on a non-nullable column would.
        var highestOrder = await dbContext.LabTests
            .Select(t => (int?)t.DisplayOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var test = LabTest.Create(request.Name, request.Description, highestOrder + 1);

        dbContext.LabTests.Add(test);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTestResponse(test.Id);
    }
}
