using FluentValidation;

namespace Prescription.Modules.Orders.Features.SubmitConsultationOpinion;

public sealed class SubmitConsultationOpinionValidator : AbstractValidator<SubmitConsultationOpinionCommand>
{
    public SubmitConsultationOpinionValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Opinion).NotEmpty().MaximumLength(4000);
    }
}
