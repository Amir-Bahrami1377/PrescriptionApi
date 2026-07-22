using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Infrastructure.Jwt;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTimeOffset ExpiresAtUtc) GenerateAccessToken(User user);
}
