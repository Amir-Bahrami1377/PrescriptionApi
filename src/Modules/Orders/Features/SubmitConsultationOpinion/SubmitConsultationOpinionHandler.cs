using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Notifications;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.SubmitConsultationOpinion;

public sealed class SubmitConsultationOpinionHandler(OrdersDbContext dbContext, IOrderStatusNotifier orderStatusNotifier)
    : IRequestHandler<SubmitConsultationOpinionCommand>
{
    public async Task Handle(SubmitConsultationOpinionCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.SubmitConsultationOpinion(request.Opinion);

        await dbContext.SaveChangesAsync(cancellationToken);

        await orderStatusNotifier.NotifyCustomerAsync(order, cancellationToken);
    }
}
