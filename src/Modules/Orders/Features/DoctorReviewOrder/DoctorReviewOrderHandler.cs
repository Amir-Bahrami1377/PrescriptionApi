using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.DoctorReviewOrder;

public sealed class DoctorReviewOrderHandler(OrdersDbContext dbContext) : IRequestHandler<DoctorReviewOrderCommand>
{
    public async Task Handle(DoctorReviewOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (request.Approve)
        {
            order.Approve(request.DoctorId);
        }
        else
        {
            order.Reject(request.DoctorId, request.RejectionReason!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
