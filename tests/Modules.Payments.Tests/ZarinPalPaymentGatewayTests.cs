using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Prescription.Modules.Payments.Infrastructure;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Payments.Tests;

public class ZarinPalPaymentGatewayTests
{
    private static readonly ZarinPalOptions Options = new()
    {
        MerchantId = "merchant-123",
        BaseUrl = "https://api.zarinpal.com/pg/v4/payment/",
        StartPayUrl = "https://www.zarinpal.com/pg/StartPay/",
    };

    private static ZarinPalPaymentGateway CreateGateway(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(Options.BaseUrl) };
        return new ZarinPalPaymentGateway(httpClient, Microsoft.Extensions.Options.Options.Create(Options), NullLogger<ZarinPalPaymentGateway>.Instance);
    }

    [Fact]
    public async Task RequestPaymentAsync_SuccessfulResponse_ReturnsRedirectUrlWithAuthority()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":{"code":100,"authority":"A00000000000000000000000000123456789"},"errors":[]}""");
        var gateway = CreateGateway(handler);

        var result = await gateway.RequestPaymentAsync(new PaymentRequest(100_000, "https://example.com/callback", "test", "09123456789"));

        result.Success.Should().BeTrue();
        result.Authority.Should().Be("A00000000000000000000000000123456789");
        result.PaymentRedirectUrl.Should().Be("https://www.zarinpal.com/pg/StartPay/A00000000000000000000000000123456789");
    }

    [Fact]
    public async Task RequestPaymentAsync_FailureResponse_ReturnsUnsuccessfulResult()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":{"code":-9},"errors":["Amount must be greater than 1000 Rials."]}""");
        var gateway = CreateGateway(handler);

        var result = await gateway.RequestPaymentAsync(new PaymentRequest(100, "https://example.com/callback", "test", "09123456789"));

        result.Success.Should().BeFalse();
        result.Authority.Should().BeNull();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(101)]
    public async Task VerifyPaymentAsync_SuccessOrAlreadyVerified_ReturnsSuccessWithReferenceId(int code)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, $$"""{"data":{"code":{{code}},"ref_id":987654321},"errors":[]}""");
        var gateway = CreateGateway(handler);

        var result = await gateway.VerifyPaymentAsync(new PaymentVerification("authority-1", 100_000));

        result.Success.Should().BeTrue();
        result.ReferenceId.Should().Be("987654321");
    }

    [Fact]
    public async Task VerifyPaymentAsync_FailureResponse_ReturnsUnsuccessfulResult()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":{"code":-11},"errors":["Authority is invalid."]}""");
        var gateway = CreateGateway(handler);

        var result = await gateway.VerifyPaymentAsync(new PaymentVerification("bad-authority", 100_000));

        result.Success.Should().BeFalse();
        result.ReferenceId.Should().BeNull();
    }

    [Fact]
    public async Task RequestPaymentAsync_WithPayerMobile_SendsItAsMetadata()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":{"code":100,"authority":"S000000000000000000000000000123456"},"errors":[]}""");
        var gateway = CreateGateway(handler);

        await gateway.RequestPaymentAsync(new PaymentRequest(100_000, "https://example.com/callback", "test", "09123456789"));

        handler.LastRequestBody.Should().Contain("\"mobile\":\"09123456789\"");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RequestPaymentAsync_WithoutPayerMobile_OmitsMetadataEntirely(string? payerMobile)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":{"code":100,"authority":"S000000000000000000000000000123456"},"errors":[]}""");
        var gateway = CreateGateway(handler);

        await gateway.RequestPaymentAsync(new PaymentRequest(100_000, "https://example.com/callback", "test", payerMobile));

        // Sending metadata with a blank mobile makes ZarinPal reject the whole request with
        // "The metadata.mobile must be a string" (code -9), which is how this surfaced live.
        handler.LastRequestBody.Should().NotContain("\"mobile\"");
    }
}
