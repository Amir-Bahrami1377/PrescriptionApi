using FluentValidation;

namespace Prescription.Modules.Orders.Features.InitiatePayment;

public sealed class InitiatePaymentValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CallbackUrl).NotEmpty();
    }
}
