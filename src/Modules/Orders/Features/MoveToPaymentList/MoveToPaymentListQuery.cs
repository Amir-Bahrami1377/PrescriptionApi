using MediatR;

namespace Prescription.Modules.Orders.Features.MoveToPaymentList;

/// <summary>Lists the customer's orders that have been approved by a doctor and are waiting to be paid for.</summary>
public sealed record MoveToPaymentListQuery(Guid CustomerId) : IRequest<IReadOnlyList<PaymentListItemDto>>;

public sealed record PaymentListItemDto(Guid OrderId, Guid LabTestId, long PriceInRials, DateTimeOffset CreatedAtUtc);
