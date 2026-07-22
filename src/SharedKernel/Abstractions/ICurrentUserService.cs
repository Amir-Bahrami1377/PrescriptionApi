namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Exposes the JWT-authenticated caller to Feature handlers without a direct dependency on
/// HttpContext. Implemented in Prescription.Api on top of IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Role { get; }

    bool IsAuthenticated { get; }
}
