using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.GetMyDoctorFee;

public sealed class GetMyDoctorFeeHandler(IdentityDbContext dbContext) : IRequestHandler<GetMyDoctorFeeQuery, GetMyDoctorFeeResponse>
{
    public async Task<GetMyDoctorFeeResponse> Handle(GetMyDoctorFeeQuery request, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.DoctorId);

        return new GetMyDoctorFeeResponse(doctor.DoctorFeeInRials);
    }
}
