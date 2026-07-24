using MediatR;
using Prescription.Modules.Catalog.Domain;
using Prescription.Modules.Catalog.Infrastructure.Persistence;

namespace Prescription.Modules.Catalog.Features.CreateTest;

public sealed class CreateTestHandler(CatalogDbContext dbContext) : IRequestHandler<CreateTestCommand, CreateTestResponse>
{
    public async Task<CreateTestResponse> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        var test = LabTest.Create(request.Name, request.Description);

        dbContext.LabTests.Add(test);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTestResponse(test.Id);
    }
}
