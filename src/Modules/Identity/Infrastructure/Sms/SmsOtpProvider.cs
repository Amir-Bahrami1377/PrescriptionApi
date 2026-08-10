using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Infrastructure.Sms;

/// <summary>
/// Delivers login codes as an ordinary text message through <see cref="ISmsProvider"/>, rather than
/// through the gateway's dedicated OTP endpoint, which the account is not approved for.
/// Reusing that provider means one HTTP client, one set of credentials and one retry policy for
/// every message the system sends.
/// </summary>
public sealed class SmsOtpProvider(ISmsProvider smsProvider) : IOtpProvider
{
    public Task SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default) =>
        smsProvider.SendAsync(phoneNumber, ComposeMessage(code), cancellationToken);

    /// <summary>
    /// Kept short deliberately: Persian is sent as Unicode, where one SMS segment is 70 characters
    /// and going over doubles the cost of every login. The validity period is left out because it
    /// is already returned to the client as expiresInSeconds, and repeating it here would be a
    /// second place to keep in step.
    /// </summary>
    private static string ComposeMessage(string code) =>
        $"سامانه نسخه\nکد ورود: {code}\nاین کد را در اختیار کسی قرار ندهید.";
}
