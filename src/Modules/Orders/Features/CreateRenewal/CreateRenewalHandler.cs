using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.CreateRenewal;

public sealed class CreateRenewalHandler(OrdersDbContext dbContext, IIdentityLookup identityLookup)
    : IRequestHandler<CreateRenewalCommand, CreateRenewalResponse>
{
    public async Task<CreateRenewalResponse> Handle(CreateRenewalCommand request, CancellationToken cancellationToken)
    {
        // Authoritative gate. The JWT carries the same flag for the client's benefit, but it goes stale
        // the moment an admin revokes access, so the decision is re-read from the database here.
        var isSpecialPatient = await identityLookup.IsSpecialPatientAsync(request.CustomerId, cancellationToken);
        if (!isSpecialPatient)
        {
            throw new UnauthorizedDomainException("تمدید نسخه فقط برای بیماران ویژه فعال است.");
        }

        var pendingApprovalCount = await dbContext.PrescriptionRenewals.CountAsync(
            r => r.CustomerId == request.CustomerId && r.Status == RenewalStatus.PendingDoctorApproval,
            cancellationToken);

        PrescriptionRenewal.EnsureCustomerCanSubmitAnotherRenewal(pendingApprovalCount);

        var renewal = PrescriptionRenewal.Create(
            request.CustomerId,
            request.CurrentPrescriptionReferenceNumber,
            request.NationalCode,
            request.BasicInsurance);

        dbContext.PrescriptionRenewals.Add(renewal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateRenewalResponse(renewal.Id, renewal.Status.ToString());
    }
}
