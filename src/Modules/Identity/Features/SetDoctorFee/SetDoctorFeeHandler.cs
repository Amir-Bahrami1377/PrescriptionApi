using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.SetDoctorFee;

public sealed class SetDoctorFeeHandler(IdentityDbContext dbContext) : IRequestHandler<SetDoctorFeeCommand>
{
    public async Task Handle(SetDoctorFeeCommand request, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.DoctorId);

        doctor.SetDoctorFee(request.FeeInRials);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
