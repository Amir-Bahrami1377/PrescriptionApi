using System.Collections.Concurrent;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.IntegrationTests.Fakes;

/// <summary>Captures the code instead of sending an SMS, so tests can read what "was sent".</summary>
public sealed class FakeOtpProvider : IOtpProvider
{
    private readonly ConcurrentDictionary<string, string> _sentCodes = new();

    public Task SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        _sentCodes[phoneNumber] = code;
        return Task.CompletedTask;
    }

    public string GetLastCodeSentTo(string phoneNumber) =>
        _sentCodes.TryGetValue(phoneNumber, out var code)
            ? code
            : throw new InvalidOperationException($"No OTP was sent to {phoneNumber}.");
}
