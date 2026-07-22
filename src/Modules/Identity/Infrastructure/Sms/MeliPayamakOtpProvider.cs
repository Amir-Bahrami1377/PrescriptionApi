using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Infrastructure.Sms;

/// <summary>
/// Sends OTP codes via MeliPayamak's BaseServiceNumber/SendByBaseNumber pattern API.
/// Retry/backoff for transient failures is applied via the named HttpClient's Polly handler
/// registered in Prescription.Api's DI composition root.
/// </summary>
public sealed class MeliPayamakOtpProvider(HttpClient httpClient, IOptions<MeliPayamakOptions> options, ILogger<MeliPayamakOtpProvider> logger)
    : IOtpProvider
{
    private readonly MeliPayamakOptions _options = options.Value;

    public async Task SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            username = _options.Username,
            password = _options.Password,
            to = phoneNumber,
            bodyId = _options.BodyId,
            text = code,
        };

        var response = await httpClient.PostAsJsonAsync("SendByBaseNumber", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MeliPayamakResponse>(cancellationToken);

        if (result is null || result.RetStatus != 1)
        {
            logger.LogError("MeliPayamak OTP send failed for {Phone}: {@Result}", phoneNumber, result);
            throw new InvalidOperationException($"MeliPayamak OTP send failed: {result?.StrRetStatus}");
        }
    }

    private sealed record MeliPayamakResponse(int RetStatus, string? StrRetStatus, string? Value);
}
