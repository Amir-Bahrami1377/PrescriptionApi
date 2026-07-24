using MediatR;

namespace Prescription.Modules.Orders.Features.GetOrderResultFile;

/// <summary>Covers both the doctor-uploaded result (regular orders) and the customer-uploaded
/// result (consultation orders) — both populate the same Order.ResultFileKey.</summary>
public sealed record GetOrderResultFileQuery(Guid OrderId, Guid RequestingUserId, string RequestingUserRole) : IRequest<GetOrderResultFileResponse>;

public sealed record GetOrderResultFileResponse(string Url, int ExpiresInSeconds);
