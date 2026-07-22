using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Jwt;
using Prescription.Modules.Identity.Infrastructure.Otp;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.VerifyOtpAndLogin;

public sealed class VerifyOtpAndLoginHandler(
    IOtpCodeStore otpCodeStore,
    IdentityDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<VerifyOtpAndLoginCommand, VerifyOtpAndLoginResponse>
{
    public async Task<VerifyOtpAndLoginResponse> Handle(VerifyOtpAndLoginCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);

        var storedHash = await otpCodeStore.GetCodeAsync(phoneNumber, cancellationToken);
        if (storedHash is null || storedHash != OtpCodeHasher.Hash(request.Code))
        {
            throw new DomainException("کد تایید نامعتبر یا منقضی شده است.");
        }

        await otpCodeStore.RemoveCodeAsync(phoneNumber, cancellationToken);

        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
        if (user is null)
        {
            user = User.RegisterFromPhoneNumber(phoneNumber);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateAccessToken(user);

        return new VerifyOtpAndLoginResponse(accessToken, expiresAtUtc, user.Role.ToString(), user.IsProfileCompleted);
    }
}
