using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListMyRenewals;

public sealed class ListMyRenewalsHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListMyRenewalsQuery, IReadOnlyList<MyRenewalDto>>
{
    public async Task<IReadOnlyList<MyRenewalDto>> Handle(ListMyRenewalsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.PrescriptionRenewals
            .AsNoTracking()
            .Where(r => r.CustomerId == request.CustomerId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new MyRenewalDto(
                r.Id,
                r.CurrentPrescriptionReferenceNumber,
                r.BasicInsurance,
                r.Status.ToString(),
                r.PriceInRials,
                r.RejectionReason,
                r.NewPrescriptionReferenceNumber,
                r.CreatedAtUtc,
                r.CompletedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
