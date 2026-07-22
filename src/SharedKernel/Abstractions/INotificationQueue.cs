namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Fire-and-forget entry point other modules use to queue an SMS without depending on the
/// Notifications module directly. Implemented by Prescription.Modules.Notifications on top of
/// a Hangfire background job so the HTTP request never blocks on the SMS gateway.
/// </summary>
public interface INotificationQueue
{
    void EnqueueSms(string phoneNumber, string message);
}
