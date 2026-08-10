using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.InitiateRenewalPayment;

public sealed class InitiateRenewalPaymentHandler(OrdersDbContext dbContext, IPaymentGateway paymentGateway)
    : IRequestHandler<InitiateRenewalPaymentCommand, InitiateRenewalPaymentResponse>
{
    public async Task<InitiateRenewalPaymentResponse> Handle(InitiateRenewalPaymentCommand request, CancellationToken cancellationToken)
    {
        var renewal = await dbContext.PrescriptionRenewals.FirstOrDefaultAsync(r => r.Id == request.RenewalId, cancellationToken)
            ?? throw new NotFoundException(nameof(PrescriptionRenewal), request.RenewalId);

        if (renewal.CustomerId != request.CustomerId)
        {
            throw new UnauthorizedDomainException("این درخواست متعلق به شما نیست.");
        }

        var paymentRequest = new PaymentRequest(renewal.RequirePrice(), request.CallbackUrl, "پرداخت هزینه تمدید نسخه", request.PayerMobile);
        var result = await paymentGateway.RequestPaymentAsync(paymentRequest, cancellationToken);

        if (!result.Success || result.Authority is null || result.PaymentRedirectUrl is null)
        {
            throw new DomainException(result.ErrorMessage ?? "ایجاد تراکنش پرداخت ناموفق بود.");
        }

        renewal.RecordPaymentInitiated(result.Authority);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new InitiateRenewalPaymentResponse(result.PaymentRedirectUrl);
    }
}
