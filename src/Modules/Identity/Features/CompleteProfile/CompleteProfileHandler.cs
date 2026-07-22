using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.CompleteProfile;

public sealed class CompleteProfileHandler(IdentityDbContext dbContext)
    : IRequestHandler<CompleteProfileCommand, CompleteProfileResponse>
{
    public async Task<CompleteProfileResponse> Handle(CompleteProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.User), request.UserId);

        user.CompleteProfile(request.NationalCode, request.FullName, request.Age, request.Gender);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CompleteProfileResponse(user.IsProfileCompleted);
    }
}
