namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Exposes the JWT-authenticated caller to Feature handlers without a direct dependency on
/// HttpContext. Implemented in Prescription.Api on top of IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Role { get; }

    /// <summary>The caller's mobile number, straight off the token — no lookup needed. ZarinPal wants
    /// it on the payment request so the payer sees their saved cards.</summary>
    string? PhoneNumber { get; }

    bool IsAuthenticated { get; }
}
