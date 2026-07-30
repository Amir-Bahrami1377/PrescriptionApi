using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.RenewalPaymentCallback;

public sealed class RenewalPaymentCallbackHandler(OrdersDbContext dbContext, IPaymentGateway paymentGateway)
    : IRequestHandler<RenewalPaymentCallbackCommand, RenewalPaymentCallbackResponse>
{
    public async Task<RenewalPaymentCallbackResponse> Handle(RenewalPaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        var renewal = await dbContext.PrescriptionRenewals.FirstOrDefaultAsync(r => r.Id == request.RenewalId, cancellationToken)
            ?? throw new NotFoundException(nameof(PrescriptionRenewal), request.RenewalId);

        // Idempotent: the gateway (or the patient's browser) may hit this callback more than once.
        if (renewal.Status != RenewalStatus.AwaitingPayment)
        {
            return new RenewalPaymentCallbackResponse(renewal.PaymentReferenceId is not null, renewal.PaymentReferenceId);
        }

        if (renewal.PaymentAuthority != request.Authority)
        {
            throw new DomainException("شناسه تراکنش با درخواست مطابقت ندارد.");
        }

        if (!string.Equals(request.Status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return new RenewalPaymentCallbackResponse(false, null);
        }

        var verification = new PaymentVerification(request.Authority, renewal.RequirePrice());
        var result = await paymentGateway.VerifyPaymentAsync(verification, cancellationToken);

        if (!result.Success || result.ReferenceId is null)
        {
            return new RenewalPaymentCallbackResponse(false, null);
        }

        renewal.ConfirmPayment(result.ReferenceId);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RenewalPaymentCallbackResponse(true, result.ReferenceId);
    }
}
