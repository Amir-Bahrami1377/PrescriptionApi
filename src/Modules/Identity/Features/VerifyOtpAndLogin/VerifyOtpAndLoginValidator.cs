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

        // The gateway generates the code, so its length is theirs to choose — MeliPayamak's own
        // documented example is ten digits. Pinning this to a fixed five rejected every real code.
        // The range is only input sanity; the hash comparison is what actually decides.
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{4,12}$")
            .WithMessage("کد تایید نامعتبر است.");
    }
}
