using MediatR;

namespace Prescription.Modules.Orders.Features.ListOrdersAwaitingConsultationOpinion;

/// <summary>Consultation orders where the customer has uploaded their test result and any doctor may now submit an opinion.</summary>
public sealed record ListOrdersAwaitingConsultationOpinionQuery : IRequest<IReadOnlyList<AwaitingConsultationOpinionOrderDto>>;

public sealed record AwaitingConsultationOpinionOrderDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    Guid? DoctorId,
    DateTimeOffset CreatedAtUtc);
