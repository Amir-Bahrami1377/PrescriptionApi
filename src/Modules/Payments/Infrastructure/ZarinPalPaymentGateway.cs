using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Payments.Infrastructure;

public sealed class ZarinPalPaymentGateway(HttpClient httpClient, IOptions<ZarinPalOptions> options, ILogger<ZarinPalPaymentGateway> logger)
    : IPaymentGateway
{
    private readonly ZarinPalOptions _options = options.Value;

    public async Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        // ZarinPal rejects the request outright if metadata.mobile is present but empty, so the
        // block is omitted entirely rather than sent blank when we have no number for the payer.
        var metadata = string.IsNullOrWhiteSpace(request.PayerMobile)
            ? null
            : new ZarinPalMetadata(request.PayerMobile);

        var payload = new ZarinPalRequestPayload(
            _options.MerchantId,
            request.AmountInRials,
            request.CallbackUrl,
            request.Description,
            metadata);

        var response = await httpClient.PostAsJsonAsync("request.json", payload, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<ZarinPalRequestResponse>(cancellationToken);

        if (!response.IsSuccessStatusCode || body?.Data is not { Code: 100, Authority: { } authority })
        {
            logger.LogError("ZarinPal payment request failed: {@Response}", body);
            return new PaymentRequestResult(false, null, null, body?.Errors?.ToString() ?? "خطا در ایجاد تراکنش پرداخت.");
        }

        var redirectUrl = $"{_options.StartPayUrl.TrimEnd('/')}/{authority}";
        return new PaymentRequestResult(true, authority, redirectUrl, null);
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(PaymentVerification verification, CancellationToken cancellationToken = default)
    {
        var payload = new ZarinPalVerifyPayload(_options.MerchantId, verification.AmountInRials, verification.Authority);

        var response = await httpClient.PostAsJsonAsync("verify.json", payload, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<ZarinPalVerifyResponse>(cancellationToken);

        // 100 = freshly verified, 101 = already verified (idempotent retry) — both are success.
        if (!response.IsSuccessStatusCode || body?.Data is not { Code: 100 or 101, RefId: { } refId })
        {
            logger.LogError("ZarinPal payment verification failed: {@Response}", body);
            return new PaymentVerificationResult(false, null, "تایید تراکنش پرداخت ناموفق بود.");
        }

        return new PaymentVerificationResult(true, refId.ToString(), null);
    }

    private sealed record ZarinPalRequestPayload(
        [property: JsonPropertyName("merchant_id")] string MerchantId,
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("callback_url")] string CallbackUrl,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("metadata")] ZarinPalMetadata? Metadata);

    private sealed record ZarinPalMetadata([property: JsonPropertyName("mobile")] string Mobile);

    private sealed record ZarinPalRequestResponse(
        [property: JsonPropertyName("data")] ZarinPalRequestData? Data,
        [property: JsonPropertyName("errors")] object? Errors);

    private sealed record ZarinPalRequestData(
        [property: JsonPropertyName("code")] int Code,
        [property: JsonPropertyName("authority")] string? Authority);

    private sealed record ZarinPalVerifyPayload(
        [property: JsonPropertyName("merchant_id")] string MerchantId,
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("authority")] string Authority);

    private sealed record ZarinPalVerifyResponse(
        [property: JsonPropertyName("data")] ZarinPalVerifyData? Data,
        [property: JsonPropertyName("errors")] object? Errors);

    private sealed record ZarinPalVerifyData(
        [property: JsonPropertyName("code")] int Code,
        [property: JsonPropertyName("ref_id")] long? RefId);
}
