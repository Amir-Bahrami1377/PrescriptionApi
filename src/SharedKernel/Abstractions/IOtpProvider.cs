namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Delivers a one-time-password code. The code is generated and verified by the Identity module;
/// this abstraction only carries it to the user, and owns how the message reads.
/// </summary>
public interface IOtpProvider
{
    Task SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
