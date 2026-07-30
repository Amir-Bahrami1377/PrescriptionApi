using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.CompleteRenewal;

public sealed class CompleteRenewalHandler(OrdersDbContext dbContext) : IRequestHandler<CompleteRenewalCommand>
{
    public async Task Handle(CompleteRenewalCommand request, CancellationToken cancellationToken)
    {
        var renewal = await dbContext.PrescriptionRenewals.FirstOrDefaultAsync(r => r.Id == request.RenewalId, cancellationToken)
            ?? throw new NotFoundException(nameof(PrescriptionRenewal), request.RenewalId);

        renewal.Complete(request.NewPrescriptionReferenceNumber);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
