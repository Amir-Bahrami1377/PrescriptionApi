using FluentValidation;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Features.CompleteProfile;

public sealed class CompleteProfileValidator : AbstractValidator<CompleteProfileCommand>
{
    public CompleteProfileValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .Must(NationalCodeValidator.IsValid)
            .WithMessage("کد ملی معتبر نیست.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120);

        RuleFor(x => x.Gender)
            .IsInEnum();
    }
}
