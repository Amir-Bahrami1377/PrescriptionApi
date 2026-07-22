using MediatR;

namespace Prescription.Modules.Identity.Features.RequestOtp;

public sealed record RequestOtpCommand(string PhoneNumber) : IRequest<RequestOtpResponse>;

public sealed record RequestOtpResponse(int ExpiresInSeconds);
