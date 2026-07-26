using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Domain;
using Prescription.Modules.Catalog.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Catalog.Features.DeleteTest;

public sealed class DeleteTestHandler(CatalogDbContext dbContext) : IRequestHandler<DeleteTestCommand>
{
    public async Task Handle(DeleteTestCommand request, CancellationToken cancellationToken)
    {
        var test = await dbContext.LabTests.FirstOrDefaultAsync(t => t.Id == request.Id && t.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(LabTest), request.Id);

        test.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
