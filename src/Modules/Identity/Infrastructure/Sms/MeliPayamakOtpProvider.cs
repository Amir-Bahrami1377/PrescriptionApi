using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Infrastructure.Sms;

/// <summary>
/// Sends OTP codes via MeliPayamak's console send/otp endpoint. The gateway composes and sends the
/// message and reports back the code it generated, so the code is a result here rather than an
/// argument — the caller stores it to verify against later.
/// Retry/backoff for transient failures comes from the named HttpClient's Polly handler registered
/// in IdentityModule.
/// </summary>
public sealed class MeliPayamakOtpProvider(HttpClient httpClient, IOptions<MeliPayamakOptions> options, ILogger<MeliPayamakOtpProvider> logger)
    : IOtpProvider
{
    private readonly MeliPayamakOptions _options = options.Value;

    public async Task<string> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        // The API key is part of the path rather than a header for this endpoint.
        var response = await httpClient.PostAsJsonAsync(
            $"api/send/otp/{_options.OtpApiKey}",
            new { to = phoneNumber },
            cancellationToken);

        // Deliberately not EnsureSuccessStatusCode: the gateway reports real problems as a 400 whose
        // body carries the only useful explanation — an account still awaiting approval answers
        // {"status":"مستلزم تنظیم و تأیید مدیر"}. Throwing on the status code alone would discard that
        // and leave nothing but "400 (Bad Request)" to work from.
        var body = await ReadBodyAsync(response, cancellationToken);

        // A populated code is the only success signal: "status" carries an error description when
        // something goes wrong, and is not a reliable indicator on its own.
        if (string.IsNullOrWhiteSpace(body?.Code))
        {
            var reason = string.IsNullOrWhiteSpace(body?.Status) ? $"HTTP {(int)response.StatusCode}" : body!.Status;
            logger.LogError("MeliPayamak OTP send failed for {Phone}: {Status}", phoneNumber, reason);
            throw new InvalidOperationException($"MeliPayamak OTP send failed: {reason}");
        }

        return body.Code;
    }

    private static async Task<MeliPayamakOtpResponse?> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<MeliPayamakOtpResponse>(cancellationToken);
        }
        catch (Exception)
        {
            // A gateway or proxy failure can answer with HTML rather than JSON; fall back to the
            // status code instead of masking it with a deserialisation error.
            return null;
        }
    }

    private sealed record MeliPayamakOtpResponse(
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("status")] string? Status);
}
