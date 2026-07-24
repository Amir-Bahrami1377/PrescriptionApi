using FluentValidation;

namespace Prescription.Modules.Identity.Features.SetDoctorFee;

public sealed class SetDoctorFeeValidator : AbstractValidator<SetDoctorFeeCommand>
{
    public SetDoctorFeeValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.FeeInRials).GreaterThan(0);
    }
}
