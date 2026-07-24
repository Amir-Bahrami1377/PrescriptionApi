using FluentValidation;

namespace Prescription.Modules.Orders.Features.UploadConsultationTestResult;

public sealed class UploadConsultationTestResultValidator : AbstractValidator<UploadConsultationTestResultCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "application/pdf"];

    public UploadConsultationTestResultValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("فرمت فایل باید JPEG، PNG یا PDF باشد.");
    }
}
