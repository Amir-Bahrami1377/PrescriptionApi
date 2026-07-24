using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.InitiatePayment;

public sealed class InitiatePaymentHandler(OrdersDbContext dbContext, IPaymentGateway paymentGateway)
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResponse>
{
    public async Task<InitiatePaymentResponse> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.CustomerId != request.CustomerId)
        {
            throw new UnauthorizedDomainException("این سفارش متعلق به شما نیست.");
        }

        var paymentRequest = new PaymentRequest(order.RequirePrice(), request.CallbackUrl, "پرداخت هزینه ویزیت", string.Empty);
        var result = await paymentGateway.RequestPaymentAsync(paymentRequest, cancellationToken);

        if (!result.Success || result.Authority is null || result.PaymentRedirectUrl is null)
        {
            throw new DomainException(result.ErrorMessage ?? "ایجاد تراکنش پرداخت ناموفق بود.");
        }

        order.RecordPaymentInitiated(result.Authority);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new InitiatePaymentResponse(result.PaymentRedirectUrl);
    }
}
