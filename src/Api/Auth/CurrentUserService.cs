using System.Security.Claims;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Api.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public string? PhoneNumber => Principal?.FindFirstValue(ClaimTypes.MobilePhone);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
