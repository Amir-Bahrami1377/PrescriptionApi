using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.AttachPrescriptionReference;

public sealed class AttachPrescriptionReferenceHandler(OrdersDbContext dbContext) : IRequestHandler<AttachPrescriptionReferenceCommand>
{
    public async Task Handle(AttachPrescriptionReferenceCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.AttachPrescriptionReference(request.PrescriptionReferenceNumber);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
