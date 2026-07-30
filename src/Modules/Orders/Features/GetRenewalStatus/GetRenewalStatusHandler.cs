using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.GetRenewalStatus;

public sealed class GetRenewalStatusHandler(OrdersDbContext dbContext) : IRequestHandler<GetRenewalStatusQuery, RenewalStatusDto>
{
    public async Task<RenewalStatusDto> Handle(GetRenewalStatusQuery request, CancellationToken cancellationToken)
    {
        var renewal = await dbContext.PrescriptionRenewals.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RenewalId, cancellationToken)
            ?? throw new NotFoundException(nameof(PrescriptionRenewal), request.RenewalId);

        var isOwner = renewal.CustomerId == request.RequestingUserId;
        var isStaff = request.RequestingUserRole is "Doctor" or "Admin";

        if (!isOwner && !isStaff)
        {
            throw new UnauthorizedDomainException("این درخواست متعلق به شما نیست.");
        }

        return new RenewalStatusDto(
            renewal.Id,
            renewal.CurrentPrescriptionReferenceNumber,
            renewal.NationalCode,
            renewal.BasicInsurance,
            renewal.Status.ToString(),
            renewal.PriceInRials,
            renewal.RejectionReason,
            renewal.PaymentReferenceId,
            renewal.NewPrescriptionReferenceNumber,
            renewal.CreatedAtUtc,
            renewal.CompletedAtUtc);
    }
}
