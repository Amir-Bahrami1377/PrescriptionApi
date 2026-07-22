using FluentValidation;

namespace Prescription.Modules.Orders.Features.AttachPrescriptionReference;

public sealed class AttachPrescriptionReferenceValidator : AbstractValidator<AttachPrescriptionReferenceCommand>
{
    public AttachPrescriptionReferenceValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.PrescriptionReferenceNumber).NotEmpty().MaximumLength(200);
    }
}
