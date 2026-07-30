using MediatR;

namespace Prescription.Modules.Identity.Features.VerifyOtpAndLogin;

public sealed record VerifyOtpAndLoginCommand(string PhoneNumber, string Code) : IRequest<VerifyOtpAndLoginResponse>;

public sealed record VerifyOtpAndLoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string Role,
    bool IsProfileCompleted,
    // Lets the client decide whether to show the prescription-renewal section.
    bool IsSpecialPatient);
