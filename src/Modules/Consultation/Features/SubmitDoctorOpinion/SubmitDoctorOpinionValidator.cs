using FluentValidation;

namespace Prescription.Modules.Consultation.Features.SubmitDoctorOpinion;

public sealed class SubmitDoctorOpinionValidator : AbstractValidator<SubmitDoctorOpinionCommand>
{
    public SubmitDoctorOpinionValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Opinion).NotEmpty().MaximumLength(4000);
    }
}
