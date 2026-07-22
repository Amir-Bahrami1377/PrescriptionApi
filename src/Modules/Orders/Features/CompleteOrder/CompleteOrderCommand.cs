using MediatR;

namespace Prescription.Modules.Orders.Features.CompleteOrder;

public sealed record CompleteOrderCommand(Guid OrderId) : IRequest;
