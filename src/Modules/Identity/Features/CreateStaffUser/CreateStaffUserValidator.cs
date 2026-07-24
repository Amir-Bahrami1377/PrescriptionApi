using FluentValidation;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Features.CreateStaffUser;

public sealed class CreateStaffUserValidator : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(?:\+98|0)?9\d{9}$")
            .WithMessage("شماره موبایل معتبر نیست.");

        RuleFor(x => x.Role)
            .Must(role => role is UserRole.Doctor or UserRole.Admin)
            .WithMessage("نقش باید پزشک یا ادمین باشد؛ مشتری از طریق ورود با کد تایید ثبت‌نام می‌شود.");
    }
}
