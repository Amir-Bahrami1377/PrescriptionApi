using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyRenewalsInProgress;

public sealed class ListMyRenewalsInProgressHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListMyRenewalsInProgressQuery, IReadOnlyList<InProgressRenewalDto>>
{
    public async Task<IReadOnlyList<InProgressRenewalDto>> Handle(ListMyRenewalsInProgressQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.PrescriptionRenewals
            .AsNoTracking()
            .Where(r => r.DoctorId == request.DoctorId && r.Status == RenewalStatus.InProgress)
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r => new InProgressRenewalDto(
                r.Id,
                r.CustomerId,
                r.CurrentPrescriptionReferenceNumber,
                r.NationalCode,
                r.BasicInsurance,
                r.PriceInRials,
                r.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
