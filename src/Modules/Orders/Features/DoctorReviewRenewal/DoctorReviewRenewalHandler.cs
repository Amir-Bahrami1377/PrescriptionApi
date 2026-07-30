using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.DoctorReviewRenewal;

public sealed class DoctorReviewRenewalHandler(OrdersDbContext dbContext, IIdentityLookup identityLookup)
    : IRequestHandler<DoctorReviewRenewalCommand>
{
    public async Task Handle(DoctorReviewRenewalCommand request, CancellationToken cancellationToken)
    {
        var renewal = await dbContext.PrescriptionRenewals.FirstOrDefaultAsync(r => r.Id == request.RenewalId, cancellationToken)
            ?? throw new NotFoundException(nameof(PrescriptionRenewal), request.RenewalId);

        if (request.Approve)
        {
            // Priced from the approving doctor's own tariff, locked onto the request at approval —
            // same rule as a lab order's visit fee.
            var fees = await identityLookup.GetDoctorFeeAsync(request.DoctorId, cancellationToken);
            if (fees?.RenewalFeeInRials is not { } feeInRials)
            {
                throw new DomainException("پزشک هنوز تعرفه تمدید نسخه خود را در پنل مدیریتی تعیین نکرده است.");
            }

            renewal.Approve(request.DoctorId, feeInRials);
        }
        else
        {
            renewal.Reject(request.DoctorId, request.RejectionReason!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
