using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListOrdersAwaitingConsultationOpinion;

/// <summary>Consultation orders where the customer has uploaded their test result and any doctor may now submit an opinion.</summary>
public sealed record ListOrdersAwaitingConsultationOpinionQuery : IRequest<IReadOnlyList<AwaitingConsultationOpinionOrderDto>>;

public sealed record AwaitingConsultationOpinionOrderDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    string? CustomerNote,
    BasicInsuranceType BasicInsurance,
    Guid? DoctorId,
    DateTimeOffset CreatedAtUtc,
    // When the customer's result upload moved this order into the queue — more useful for triage than CreatedAtUtc.
    DateTimeOffset UpdatedAtUtc);
