namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Sends a one-time-password code through the SMS gateway's dedicated OTP/pattern API
/// (distinct from a plain text SMS send via <see cref="ISmsProvider"/>).
/// The code itself is generated and verified by the Identity module against Redis; this
/// abstraction is only responsible for delivery.
/// </summary>
public interface IOtpProvider
{
    Task SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
