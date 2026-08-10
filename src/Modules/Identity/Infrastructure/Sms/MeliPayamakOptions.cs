namespace Prescription.Modules.Identity.Infrastructure.Sms;

public sealed class MeliPayamakOptions
{
    /// <summary>Identifies the account and sits in the request path, so it is a credential in its own
    /// right — it belongs in configuration, never in the repository.</summary>
    public required string OtpApiKey { get; init; }

    public string BaseUrl { get; init; } = "https://console.melipayamak.com/";
}
