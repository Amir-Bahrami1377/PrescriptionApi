namespace Prescription.Modules.Identity.Infrastructure.Jwt;

public sealed class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    /// <summary>Base64-encoded signing key, must come from a secret store.</summary>
    public required string SigningKey { get; init; }

    public int AccessTokenExpiryMinutes { get; init; } = 60;
}
