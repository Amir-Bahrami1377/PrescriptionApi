using Microsoft.Extensions.Logging;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Notifications.Features.SendSms;

public sealed class SmsJob(ISmsProvider smsProvider, ILogger<SmsJob> logger) : ISmsJob
{
    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        try
        {
            await smsProvider.SendAsync(phoneNumber, message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deliver SMS to {Phone}", phoneNumber);
            throw; // rethrow so Hangfire's automatic retry policy kicks in
        }
    }
}
