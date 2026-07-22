namespace Prescription.Modules.Notifications.Features.SendSms;

/// <summary>The unit Hangfire actually invokes; kept as an interface so Hangfire can serialize the call by type.</summary>
public interface ISmsJob
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken);
}
