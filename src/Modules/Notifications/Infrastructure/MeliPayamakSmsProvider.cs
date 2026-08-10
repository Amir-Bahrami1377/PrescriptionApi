using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Notifications.Infrastructure;

/// <summary>Sends plain text SMS via MeliPayamak's SendSMS endpoint, which is also what carries login
/// codes (see SmsOtpProvider) because the account is not approved for the dedicated OTP API.</summary>
public sealed class MeliPayamakSmsProvider(HttpClient httpClient, IOptions<MeliPayamakOptions> options, ILogger<MeliPayamakSmsProvider> logger)
    : ISmsProvider
{
    private readonly MeliPayamakOptions _options = options.Value;

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // Form-encoded, not JSON: this endpoint is form-only, as MeliPayamak's own C# client shows
        // (it posts FormUrlEncodedContent). Sending JSON here fails.
        var payload = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = _options.Username,
            ["password"] = _options.Password,
            ["to"] = phoneNumber,
            ["from"] = _options.SenderNumber,
            ["text"] = message,
            ["isFlash"] = "false",
        });

        var response = await httpClient.PostAsync("SendSMS", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MeliPayamakResponse>(cancellationToken);

        // RetStatus 1 is the only success; anything else puts the reason in StrRetStatus, and Value
        // carries the message id on success or an error code otherwise.
        if (result is null || result.RetStatus != 1)
        {
            logger.LogError(
                "MeliPayamak SMS send failed sending from {From} to {To}: {Status} ({Value})",
                _options.SenderNumber, phoneNumber, result?.StrRetStatus, result?.Value);

            // Both numbers are named because the gateway answers "InvalidNumber" for either of them,
            // and the sender line — which has to be one the account actually owns — is the usual
            // culprit. Neither is a credential.
            throw new InvalidOperationException(
                $"MeliPayamak SMS send failed: {result?.StrRetStatus ?? "unknown error"} (from '{_options.SenderNumber}' to '{phoneNumber}')");
        }
    }

    private sealed record MeliPayamakResponse(
        [property: JsonPropertyName("RetStatus")] int RetStatus,
        [property: JsonPropertyName("StrRetStatus")] string? StrRetStatus,
        [property: JsonPropertyName("Value")] string? Value);
}
