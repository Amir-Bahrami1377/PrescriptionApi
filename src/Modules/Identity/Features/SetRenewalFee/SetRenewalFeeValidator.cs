using FluentValidation;

namespace Prescription.Modules.Identity.Features.SetRenewalFee;

public sealed class SetRenewalFeeValidator : AbstractValidator<SetRenewalFeeCommand>
{
    public SetRenewalFeeValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.FeeInRials).GreaterThan(0);
    }
}
