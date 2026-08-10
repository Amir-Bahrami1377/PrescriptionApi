namespace Prescription.Modules.Notifications.Infrastructure;

public sealed class MeliPayamakOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string SenderNumber { get; init; }

    /// <summary>Named apart from the OTP provider's BaseUrl because both bind the same MeliPayamak
    /// configuration section, and plain SMS still goes through the older panel API on a different
    /// host than the OTP endpoint.</summary>
    public string SmsBaseUrl { get; init; } = "https://rest.payamak-panel.com/api/SendSMS/";
}
