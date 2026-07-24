using MediatR;

namespace Prescription.Modules.Orders.Features.ListMyOrdersAwaitingPayment;

/// <summary>Orders this doctor approved that are now waiting for the customer to pay.</summary>
public sealed record ListMyOrdersAwaitingPaymentQuery(Guid DoctorId) : IRequest<IReadOnlyList<AwaitingPaymentOrderDto>>;

public sealed record AwaitingPaymentOrderDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    long PriceInRials,
    DateTimeOffset CreatedAtUtc);
