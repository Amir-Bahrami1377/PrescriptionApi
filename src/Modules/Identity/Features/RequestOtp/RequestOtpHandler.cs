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

    public async Task<RequestOtpResponse> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);

        var requestCount = await otpCodeStore.IncrementRequestCountAsync(phoneNumber, RateLimitWindow, cancellationToken);
        if (requestCount > MaxRequestsPerWindow)
        {
            throw new RateLimitExceededException("تعداد درخواست‌های کد تایید بیش از حد مجاز است. لطفاً بعداً تلاش کنید.", RateLimitWindow);
        }

        var code = GenerateNumericCode(5);
        var codeHash = OtpCodeHasher.Hash(code);

        await otpCodeStore.SaveCodeAsync(phoneNumber, codeHash, CodeTtl, cancellationToken);
        await otpProvider.SendOtpAsync(phoneNumber, code, cancellationToken);

        return new RequestOtpResponse((int)CodeTtl.TotalSeconds);
    }

    private static string GenerateNumericCode(int length)
    {
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
