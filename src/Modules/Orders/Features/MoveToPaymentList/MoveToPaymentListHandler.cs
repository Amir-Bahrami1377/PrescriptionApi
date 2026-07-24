using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.MoveToPaymentList;

public sealed class MoveToPaymentListHandler(OrdersDbContext dbContext)
    : IRequestHandler<MoveToPaymentListQuery, IReadOnlyList<PaymentListItemDto>>
{
    public async Task<IReadOnlyList<PaymentListItemDto>> Handle(MoveToPaymentListQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == request.CustomerId && o.Status == OrderStatus.AwaitingPayment)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new PaymentListItemDto(o.Id, o.LabTestId, o.PriceInRials!.Value, o.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
