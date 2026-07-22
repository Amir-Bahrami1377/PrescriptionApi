namespace Prescription.Modules.Identity.Infrastructure.Sms;

public sealed class MeliPayamakOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BodyId { get; init; }
    public string BaseUrl { get; init; } = "https://rest.payamak-panel.com/api/SendSMS/";
}
