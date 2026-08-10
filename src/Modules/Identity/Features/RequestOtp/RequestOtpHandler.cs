using System.Security.Cryptography;
using MediatR;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Otp;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.RequestOtp;

public sealed class RequestOtpHandler(IOtpCodeStore otpCodeStore, IOtpProvider otpProvider)
    : IRequestHandler<RequestOtpCommand, RequestOtpResponse>
{
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);
    private const int MaxRequestsPerWindow = 3;

    // Five digits, matching what the client's code-entry field was built for. Brute force is not the
    // constraint here: three requests per ten minutes and a two-minute lifetime bound it far tighter
    // than the digit count does.
    private const int CodeLength = 5;

    public async Task<RequestOtpResponse> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);

        var requestCount = await otpCodeStore.IncrementRequestCountAsync(phoneNumber, RateLimitWindow, cancellationToken);
        if (requestCount > MaxRequestsPerWindow)
        {
            throw new RateLimitExceededException("تعداد درخواست‌های کد تایید بیش از حد مجاز است. لطفاً بعداً تلاش کنید.", RateLimitWindow);
        }

        var code = GenerateNumericCode(CodeLength);

        // Sent before storing, so a gateway failure leaves no code behind to verify against rather
        // than stranding the caller with one that never arrived.
        await otpProvider.SendOtpAsync(phoneNumber, code, cancellationToken);

        await otpCodeStore.SaveCodeAsync(phoneNumber, OtpCodeHasher.Hash(code), CodeTtl, cancellationToken);

        return new RequestOtpResponse((int)CodeTtl.TotalSeconds);
    }

    private static string GenerateNumericCode(int length)
    {
        // RandomNumberGenerator rather than Random: this is a credential, however short-lived.
        Span<byte> buffer = stackalloc byte[length];
        RandomNumberGenerator.Fill(buffer);

        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = (char)('0' + buffer[i] % 10);
        }

        return new string(chars);
    }
}
