using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.DeleteUser;

public sealed class DeleteUserHandler(IdentityDbContext dbContext) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.RequestingAdminId)
        {
            throw new DomainException("حذف حساب کاربری خودتان امکان‌پذیر نیست.");
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
