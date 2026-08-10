namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Sends a one-time-password code through the SMS gateway's dedicated OTP API (distinct from a plain
/// text SMS send via <see cref="ISmsProvider"/>).
/// </summary>
public interface IOtpProvider
{
    /// <summary>
    /// Sends a code to the given number and returns the code that was sent, for the caller to store
    /// and later verify against. The gateway generates the code itself rather than accepting one, so
    /// it can only be learned from the send response — the Identity module never picks it.
    /// </summary>
    Task<string> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
