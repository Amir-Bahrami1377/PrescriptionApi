using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyPendingRenewalReviews;

public sealed class ListMyPendingRenewalReviewsHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListMyPendingRenewalReviewsQuery, IReadOnlyList<MyPendingRenewalReviewDto>>
{
    public async Task<IReadOnlyList<MyPendingRenewalReviewDto>> Handle(ListMyPendingRenewalReviewsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.PrescriptionRenewals
            .AsNoTracking()
            .Where(r => r.Status == RenewalStatus.PendingDoctorApproval
                && r.ClaimedByDoctorId == request.DoctorId
                && r.ClaimExpiresAtUtc > now)
            .OrderBy(r => r.ClaimExpiresAtUtc)
            .Select(r => new MyPendingRenewalReviewDto(
                r.Id,
                r.CustomerId,
                r.CurrentPrescriptionReferenceNumber,
                r.NationalCode,
                r.BasicInsurance,
                r.ClaimExpiresAtUtc!.Value,
                r.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
