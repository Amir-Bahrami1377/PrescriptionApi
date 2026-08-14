using System.Collections.Concurrent;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.IntegrationTests.Fakes;

public sealed record QueuedSms(string PhoneNumber, string Message);

public sealed class FakeNotificationQueue : INotificationQueue
{
    private readonly ConcurrentQueue<QueuedSms> _notifications = new();

    public IReadOnlyCollection<QueuedSms> Notifications => _notifications.ToArray();

    public void EnqueueSms(string phoneNumber, string message) =>
        _notifications.Enqueue(new QueuedSms(phoneNumber, message));
}
