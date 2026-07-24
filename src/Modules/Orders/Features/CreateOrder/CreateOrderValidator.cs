using FluentValidation;

namespace Prescription.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "application/pdf"];

    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.LabTestIds)
            .NotEmpty()
            .WithMessage("انتخاب حداقل یک آزمایش الزامی است.");

        RuleForEach(x => x.LabTestIds).NotEmpty();

        RuleFor(x => x.Note).MaximumLength(2000);

        // The attachment itself is optional, but if one is provided it must be well-formed.
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .When(x => x.FileContent is not null);

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .When(x => x.FileContent is not null)
            .WithMessage("فرمت فایل باید JPEG، PNG یا PDF باشد.");
    }
}
