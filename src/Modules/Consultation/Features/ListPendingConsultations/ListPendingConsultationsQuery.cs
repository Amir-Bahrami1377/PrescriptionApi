using MediatR;

namespace Prescription.Modules.Consultation.Features.ListPendingConsultations;

public sealed record ListPendingConsultationsQuery : IRequest<IReadOnlyList<PendingConsultationDto>>;

public sealed record PendingConsultationDto(Guid Id, Guid CustomerId, string? CustomerNote, DateTimeOffset CreatedAtUtc);
