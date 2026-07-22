using FluentValidation;

namespace Prescription.Modules.Orders.Features.UploadTestResult;

public sealed class UploadTestResultValidator : AbstractValidator<UploadTestResultCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "application/pdf"];

    public UploadTestResultValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("فرمت فایل باید JPEG، PNG یا PDF باشد.");
    }
}
