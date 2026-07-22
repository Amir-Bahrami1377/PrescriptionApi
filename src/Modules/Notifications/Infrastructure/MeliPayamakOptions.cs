namespace Prescription.Modules.Notifications.Infrastructure;

public sealed class MeliPayamakOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string SenderNumber { get; init; }
    public string BaseUrl { get; init; } = "https://rest.payamak-panel.com/api/SendSMS/";
}
