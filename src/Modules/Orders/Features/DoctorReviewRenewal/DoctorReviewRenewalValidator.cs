using FluentValidation;

namespace Prescription.Modules.Orders.Features.DoctorReviewRenewal;

public sealed class DoctorReviewRenewalValidator : AbstractValidator<DoctorReviewRenewalCommand>
{
    public DoctorReviewRenewalValidator()
    {
        RuleFor(x => x.RenewalId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Approve)
            .WithMessage("در صورت رد درخواست، ذکر دلیل الزامی است.");
    }
}
