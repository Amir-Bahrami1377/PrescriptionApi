using FluentValidation;

namespace Prescription.Modules.Consultation.Features.RequestConsultation;

public sealed class RequestConsultationValidator : AbstractValidator<RequestConsultationCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png"];

    public RequestConsultationValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("فرمت تصویر باید JPEG یا PNG باشد.");
    }
}
