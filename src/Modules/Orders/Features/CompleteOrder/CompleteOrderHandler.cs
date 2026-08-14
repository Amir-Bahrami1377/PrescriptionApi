using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Notifications;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.CompleteOrder;

public sealed class CompleteOrderHandler(OrdersDbContext dbContext, IOrderStatusNotifier orderStatusNotifier)
    : IRequestHandler<CompleteOrderCommand>
{
    public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);

        await orderStatusNotifier.NotifyCustomerAsync(order, cancellationToken);
    }
}
