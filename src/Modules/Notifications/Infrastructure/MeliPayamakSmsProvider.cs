using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Notifications.Infrastructure;

/// <summary>Sends plain-text SMS via MeliPayamak's classic SendSMS endpoint (as opposed to the OTP pattern API used by Identity).</summary>
public sealed class MeliPayamakSmsProvider(HttpClient httpClient, IOptions<MeliPayamakOptions> options, ILogger<MeliPayamakSmsProvider> logger)
    : ISmsProvider
{
    private readonly MeliPayamakOptions _options = options.Value;

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            username = _options.Username,
            password = _options.Password,
            to = phoneNumber,
            from = _options.SenderNumber,
            text = message,
            isFlash = false,
        };

        var response = await httpClient.PostAsJsonAsync("SendSMS", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MeliPayamakResponse>(cancellationToken);

        if (result is null || result.RetStatus != 1)
        {
            logger.LogError("MeliPayamak SMS send failed for {Phone}: {@Result}", phoneNumber, result);
            throw new InvalidOperationException($"MeliPayamak SMS send failed: {result?.StrRetStatus}");
        }
    }

    private sealed record MeliPayamakResponse(int RetStatus, string? StrRetStatus, string? Value);
}
