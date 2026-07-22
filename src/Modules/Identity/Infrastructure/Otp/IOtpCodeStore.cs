namespace Prescription.Modules.Identity.Infrastructure.Otp;

public interface IOtpCodeStore
{
    Task SaveCodeAsync(string phoneNumber, string code, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task<string?> GetCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);

    Task RemoveCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>Increments and returns the number of OTP requests made for this phone number within the current rate-limit window.</summary>
    Task<int> IncrementRequestCountAsync(string phoneNumber, TimeSpan window, CancellationToken cancellationToken = default);
}
