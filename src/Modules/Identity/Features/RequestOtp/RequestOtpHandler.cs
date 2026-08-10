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

        // The gateway generates the code as part of sending it, so it is only known once the send has
        // succeeded. Storing after the fact also means a failed send leaves no code behind to verify
        // against, rather than stranding the caller with one that never arrived.
        var code = await otpProvider.SendOtpAsync(phoneNumber, cancellationToken);

        await otpCodeStore.SaveCodeAsync(phoneNumber, OtpCodeHasher.Hash(code), CodeTtl, cancellationToken);

        return new RequestOtpResponse((int)CodeTtl.TotalSeconds);
    }
}
