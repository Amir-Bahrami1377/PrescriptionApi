using MediatR;

namespace Prescription.Modules.Consultation.Features.RequestConsultation;

public sealed record RequestConsultationCommand(
    Guid CustomerId,
    string? Note,
    Stream FileContent,
    string FileName,
    string ContentType) : IRequest<RequestConsultationResponse>;

public sealed record RequestConsultationResponse(Guid ConsultationId);
