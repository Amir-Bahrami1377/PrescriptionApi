using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyPendingReviews;

public sealed class ListMyPendingReviewsHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListMyPendingReviewsQuery, IReadOnlyList<MyPendingReviewDto>>
{
    public async Task<IReadOnlyList<MyPendingReviewDto>> Handle(ListMyPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.PendingDoctorApproval
                && o.ClaimedByDoctorId == request.DoctorId
                && o.ClaimExpiresAtUtc > now)
            .OrderBy(o => o.ClaimExpiresAtUtc)
            .Select(o => new MyPendingReviewDto(
                o.Id,
                o.CustomerId,
                o.LabTestIds,
                o.CustomerNote,
                o.CustomerUploadedFileKey != null,
                o.ClaimExpiresAtUtc!.Value,
                o.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
