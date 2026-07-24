using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.SetConsultationFee;

public sealed class SetConsultationFeeHandler(IdentityDbContext dbContext) : IRequestHandler<SetConsultationFeeCommand>
{
    public async Task Handle(SetConsultationFeeCommand request, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.DoctorId);

        doctor.SetConsultationFee(request.FeeInRials);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
