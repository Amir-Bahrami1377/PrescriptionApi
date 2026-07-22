using FluentValidation;

namespace Prescription.Modules.Identity.Features.RequestOtp;

public sealed class RequestOtpValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(?:\+98|0)?9\d{9}$")
            .WithMessage("شماره موبایل معتبر نیست.");
    }
}
