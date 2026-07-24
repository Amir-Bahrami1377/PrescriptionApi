using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;

namespace Prescription.Modules.Identity.Features.CreateStaffUser;

public sealed class CreateStaffUserHandler(IdentityDbContext dbContext)
    : IRequestHandler<CreateStaffUserCommand, CreateStaffUserResponse>
{
    public async Task<CreateStaffUserResponse> Handle(CreateStaffUserCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);

        if (user is not null)
        {
            user.ChangeRole(request.Role);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateStaffUserResponse(user.Id, user.Role.ToString(), Created: false);
        }

        user = User.RegisterFromPhoneNumber(phoneNumber, request.Role);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateStaffUserResponse(user.Id, user.Role.ToString(), Created: true);
    }
}
