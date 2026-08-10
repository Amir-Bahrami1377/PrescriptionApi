using FluentAssertions;
using NSubstitute;
using Prescription.Modules.Identity.Infrastructure.Sms;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Tests;

public class SmsOtpProviderTests
{
    private static (SmsOtpProvider Provider, ISmsProvider Sms) Create()
    {
        var sms = Substitute.For<ISmsProvider>();
        return (new SmsOtpProvider(sms), sms);
    }

    private static async Task<string> CaptureMessageAsync(string code)
    {
        var (provider, sms) = Create();
        await provider.SendOtpAsync("09121112233", code);

        var calls = sms.ReceivedCalls().ToList();
        calls.Should().HaveCount(1);
        return (string)calls[0].GetArguments()[1]!;
    }

    [Fact]
    public async Task SendOtpAsync_SendsToTheGivenNumber()
    {
        var (provider, sms) = Create();

        await provider.SendOtpAsync("09121112233", "12345");

        await sms.Received(1).SendAsync("09121112233", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOtpAsync_PutsTheCodeInTheMessage()
    {
        var message = await CaptureMessageAsync("12345");

        message.Should().Contain("12345");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("123456789012")]
    public async Task SendOtpAsync_MessageFitsOneUnicodeSegment(string code)
    {
        var message = await CaptureMessageAsync(code);

        // Persian goes out as UCS-2, where a single SMS segment is 70 characters. Spilling over
        // silently doubles the cost of every login, so the ceiling is worth pinning down — checked
        // against the longest code the verification rule accepts, not just the current length.
        message.Length.Should().BeLessThanOrEqualTo(70);
    }
}
