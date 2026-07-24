using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyOrdersAwaitingPayment;

public sealed class ListMyOrdersAwaitingPaymentHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListMyOrdersAwaitingPaymentQuery, IReadOnlyList<AwaitingPaymentOrderDto>>
{
    public async Task<IReadOnlyList<AwaitingPaymentOrderDto>> Handle(ListMyOrdersAwaitingPaymentQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.DoctorId == request.DoctorId && o.Status == OrderStatus.AwaitingPayment)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AwaitingPaymentOrderDto(o.Id, o.CustomerId, o.LabTestIds, o.PriceInRials!.Value, o.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
