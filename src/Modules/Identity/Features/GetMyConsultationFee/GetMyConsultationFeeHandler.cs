using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.GetMyConsultationFee;

public sealed class GetMyConsultationFeeHandler(IdentityDbContext dbContext) : IRequestHandler<GetMyConsultationFeeQuery, GetMyConsultationFeeResponse>
{
    public async Task<GetMyConsultationFeeResponse> Handle(GetMyConsultationFeeQuery request, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.DoctorId);

        return new GetMyConsultationFeeResponse(doctor.ConsultationFeeInRials);
    }
}
