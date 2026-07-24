using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyOrdersInProgress;

public sealed class ListMyOrdersInProgressHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListMyOrdersInProgressQuery, IReadOnlyList<InProgressOrderDto>>
{
    public async Task<IReadOnlyList<InProgressOrderDto>> Handle(ListMyOrdersInProgressQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.DoctorId == request.DoctorId && o.Status == OrderStatus.InProgress)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new InProgressOrderDto(o.Id, o.CustomerId, o.LabTestIds, o.ResultFileKey != null, o.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
