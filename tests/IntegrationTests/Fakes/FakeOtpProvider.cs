using System.Collections.Concurrent;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.IntegrationTests.Fakes;

/// <summary>Stands in for the MeliPayamak API, generating the code itself the way the real gateway
/// does, so tests can read the code that "was sent".</summary>
public sealed class FakeOtpProvider : IOtpProvider
{
    private readonly ConcurrentDictionary<string, string> _sentCodes = new();
    private int _counter;

    public Task<string> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        // Ten digits, matching the length MeliPayamak documents, so the flow is exercised against a
        // realistic code rather than one that happens to fit an assumption. Distinct per send, so a
        // test can't pass by coincidence against a constant.
        var code = (3741437400 + Interlocked.Increment(ref _counter)).ToString();
        _sentCodes[phoneNumber] = code;
        return Task.FromResult(code);
    }

    public string GetLastCodeSentTo(string phoneNumber) =>
        _sentCodes.TryGetValue(phoneNumber, out var code)
            ? code
            : throw new InvalidOperationException($"No OTP was sent to {phoneNumber}.");
}
