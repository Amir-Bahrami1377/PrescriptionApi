using FluentValidation;

namespace Prescription.Modules.Identity.Features.SetConsultationFee;

public sealed class SetConsultationFeeValidator : AbstractValidator<SetConsultationFeeCommand>
{
    public SetConsultationFeeValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.FeeInRials).GreaterThan(0);
    }
}
