using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Domain;
using Prescription.Modules.Catalog.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Catalog.Features.UpdateTest;

public sealed class UpdateTestHandler(CatalogDbContext dbContext) : IRequestHandler<UpdateTestCommand>
{
    public async Task Handle(UpdateTestCommand request, CancellationToken cancellationToken)
    {
        var test = await dbContext.LabTests.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(LabTest), request.Id);

        test.Update(request.Name, request.Description, request.PriceInRials, request.IsActive);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
