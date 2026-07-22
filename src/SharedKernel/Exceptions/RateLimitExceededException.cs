namespace Prescription.SharedKernel.Exceptions;

/// <summary>Mapped to HTTP 429. Used for OTP request rate limiting.</summary>
public sealed class RateLimitExceededException(string message, TimeSpan? retryAfter = null) : Exception(message)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
