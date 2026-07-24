using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListPendingOrders;

public sealed class ListPendingOrdersHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListPendingOrdersQuery, IReadOnlyList<PendingOrderDto>>
{
    public async Task<IReadOnlyList<PendingOrderDto>> Handle(ListPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.PendingDoctorApproval
                && (o.ClaimedByDoctorId == null || o.ClaimExpiresAtUtc <= now))
            .OrderBy(o => o.CreatedAtUtc)
            .Select(o => new PendingOrderDto(
                o.Id,
                o.CustomerId,
                o.LabTestIds,
                o.CustomerNote,
                o.CustomerUploadedFileKey != null,
                o.BasicInsurance,
                o.SupplementaryInsurance,
                o.IsForThirdParty,
                o.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
