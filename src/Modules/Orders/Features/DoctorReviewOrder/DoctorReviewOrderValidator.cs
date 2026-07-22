using FluentValidation;

namespace Prescription.Modules.Orders.Features.DoctorReviewOrder;

public sealed class DoctorReviewOrderValidator : AbstractValidator<DoctorReviewOrderCommand>
{
    public DoctorReviewOrderValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Approve)
            .WithMessage("در صورت رد سفارش، ذکر دلیل الزامی است.");
    }
}
