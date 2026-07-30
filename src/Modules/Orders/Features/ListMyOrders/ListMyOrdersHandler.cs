using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyOrders;

public sealed class ListMyOrdersHandler(OrdersDbContext dbContext) : IRequestHandler<ListMyOrdersQuery, MyOrdersResponse>
{
    public async Task<MyOrdersResponse> Handle(ListMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new MyOrderDto(
                o.Id,
                o.LabTestIds,
                o.BasicInsurance,
                o.IsForThirdParty,
                o.RequestsConsultation,
                o.Status.ToString(),
                o.PriceInRials,
                o.RejectionReason,
                o.PrescriptionReferenceNumber,
                o.ResultFileKey != null,
                o.ConsultationOpinion,
                o.CreatedAtUtc,
                o.CompletedAtUtc))
            .ToListAsync(cancellationToken);

        // Counted from the rows just fetched rather than a second query, so the figure can't disagree
        // with the list the client is rendering it next to.
        var used = orders.Count(o => o.Status == nameof(OrderStatus.PendingDoctorApproval));
        var limit = Order.MaxPendingApprovalPerCustomer;

        var capacity = new PendingApprovalCapacityDto(used, limit, Math.Max(0, limit - used));

        return new MyOrdersResponse(orders, capacity);
    }
}
