using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyList<Guid> LabTestIds,
    string? Note,
    Stream? FileContent,
    string? FileName,
    string? ContentType,
    BasicInsuranceType BasicInsurance,
    SupplementaryInsuranceType SupplementaryInsurance,
    bool IsForThirdParty,
    string? ThirdPartyNationalCode,
    string? ThirdPartyPhoneNumber,
    bool RequestsConsultation) : IRequest<CreateOrderResponse>;

public sealed record CreateOrderResponse(Guid OrderId, string Status);
