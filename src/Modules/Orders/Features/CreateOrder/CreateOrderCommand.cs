using MediatR;

namespace Prescription.Modules.Orders.Features.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    Guid LabTestId,
    string? Note,
    Stream FileContent,
    string FileName,
    string ContentType) : IRequest<CreateOrderResponse>;

public sealed record CreateOrderResponse(Guid OrderId, string Status);
