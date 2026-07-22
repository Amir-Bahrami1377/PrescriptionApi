using FluentValidation;

namespace Prescription.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "application/pdf"];

    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.LabTestId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("فرمت فایل باید JPEG، PNG یا PDF باشد.");
    }
}
