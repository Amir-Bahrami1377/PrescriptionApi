using Hangfire;
using Prescription.Modules.Notifications.Features.SendSms;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Notifications.Infrastructure;

public sealed class HangfireNotificationQueue(IBackgroundJobClient backgroundJobClient) : INotificationQueue
{
    public void EnqueueSms(string phoneNumber, string message) =>
        backgroundJobClient.Enqueue<ISmsJob>(job => job.SendAsync(phoneNumber, message, CancellationToken.None));
}
