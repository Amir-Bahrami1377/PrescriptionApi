using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Features.CompleteProfile;
using Prescription.Modules.Identity.Features.RequestOtp;
using Prescription.Modules.Identity.Features.VerifyOtpAndLogin;

namespace Prescription.IntegrationTests;

/// <summary>
/// End-to-end OTP request -> verify -> login -> complete profile flow against a real Postgres +
/// Redis (via Testcontainers), with only the outbound MeliPayamak call faked.
/// </summary>
public sealed class IdentityFlowTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private const string PhoneNumber = "09123456789";

    [Fact]
    public async Task FullOtpAndProfileFlow_Succeeds()
    {
        using var client = factory.CreateClient();

        var requestOtpResponse = await client.PostAsJsonAsync("/api/auth/otp/request", new { PhoneNumber });
        requestOtpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var requestOtpBody = await requestOtpResponse.Content.ReadFromJsonAsync<RequestOtpResponse>();
        requestOtpBody!.ExpiresInSeconds.Should().BeGreaterThan(0);

        var code = factory.FakeOtpProvider.GetLastCodeSentTo(PhoneNumber);

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/otp/verify", new { PhoneNumber, Code = code });
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<VerifyOtpAndLoginResponse>();
        verifyBody!.AccessToken.Should().NotBeNullOrWhiteSpace();
        verifyBody.Role.Should().Be("Customer");
        verifyBody.IsProfileCompleted.Should().BeFalse();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifyBody.AccessToken);

        var completeProfileResponse = await client.PostAsJsonAsync(
            "/api/auth/profile/complete",
            new { NationalCode = "0499370899", FullName = "علی رضایی", Age = 30, Gender = Gender.Male });

        completeProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeProfileBody = await completeProfileResponse.Content.ReadFromJsonAsync<CompleteProfileResponse>();
        completeProfileBody!.IsProfileCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_WrongCode_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();

        const string phoneNumber = "09121112233";
        await client.PostAsJsonAsync("/api/auth/otp/request", new { PhoneNumber = phoneNumber });

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/otp/verify", new { PhoneNumber = phoneNumber, Code = "00000" });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteProfile_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/profile/complete",
            new { NationalCode = "0499370899", FullName = "علی رضایی", Age = 30, Gender = Gender.Male });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
