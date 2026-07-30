using FluentValidation;
using Prescription.SharedKernel.Validation;

namespace Prescription.Modules.Orders.Features.CreateRenewal;

public sealed class CreateRenewalValidator : AbstractValidator<CreateRenewalCommand>
{
    public CreateRenewalValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.CurrentPrescriptionReferenceNumber)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("ثبت کد رهگیری نسخه الزامی است.");

        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .Must(nationalCode => NationalCodeValidator.IsValid(nationalCode))
            .WithMessage("کد ملی معتبر نیست.");

        RuleFor(x => x.BasicInsurance).IsInEnum();
    }
}
