using FluentValidation;

namespace Prescription.Modules.Identity.Features.VerifyOtpAndLogin;

public sealed class VerifyOtpAndLoginValidator : AbstractValidator<VerifyOtpAndLoginCommand>
{
    public VerifyOtpAndLoginValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(?:\+98|0)?9\d{9}$")
            .WithMessage("شماره موبایل معتبر نیست.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{5}$")
            .WithMessage("کد تایید باید ۵ رقم باشد.");
    }
}
