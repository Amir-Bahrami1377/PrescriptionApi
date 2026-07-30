using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.SetSpecialPatient;

public sealed class SetSpecialPatientHandler(IdentityDbContext dbContext) : IRequestHandler<SetSpecialPatientCommand>
{
    public async Task Handle(SetSpecialPatientCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.SetSpecialPatient(request.IsSpecialPatient);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
