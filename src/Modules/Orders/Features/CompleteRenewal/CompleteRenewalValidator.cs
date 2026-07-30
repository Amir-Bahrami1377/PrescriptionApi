using FluentValidation;

namespace Prescription.Modules.Orders.Features.CompleteRenewal;

public sealed class CompleteRenewalValidator : AbstractValidator<CompleteRenewalCommand>
{
    public CompleteRenewalValidator()
    {
        RuleFor(x => x.RenewalId).NotEmpty();

        RuleFor(x => x.NewPrescriptionReferenceNumber)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("برای تکمیل درخواست، ثبت کد رهگیری نسخه جدید الزامی است.");
    }
}
