using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListPendingRenewals;

public sealed class ListPendingRenewalsHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListPendingRenewalsQuery, IReadOnlyList<PendingRenewalDto>>
{
    public async Task<IReadOnlyList<PendingRenewalDto>> Handle(ListPendingRenewalsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.PrescriptionRenewals
            .AsNoTracking()
            .Where(r => r.Status == RenewalStatus.PendingDoctorApproval
                && (r.ClaimedByDoctorId == null || r.ClaimExpiresAtUtc <= now))
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r => new PendingRenewalDto(
                r.Id,
                r.CustomerId,
                r.CurrentPrescriptionReferenceNumber,
                r.NationalCode,
                r.BasicInsurance,
                r.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
