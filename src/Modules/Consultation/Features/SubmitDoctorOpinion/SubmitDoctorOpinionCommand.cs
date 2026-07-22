using MediatR;

namespace Prescription.Modules.Consultation.Features.SubmitDoctorOpinion;

public sealed record SubmitDoctorOpinionCommand(Guid ConsultationId, Guid DoctorId, string Opinion) : IRequest;
