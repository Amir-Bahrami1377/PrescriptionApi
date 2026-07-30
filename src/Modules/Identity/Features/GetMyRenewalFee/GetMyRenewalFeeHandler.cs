using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Features.GetMyRenewalFee;

public sealed class GetMyRenewalFeeHandler(IdentityDbContext dbContext) : IRequestHandler<GetMyRenewalFeeQuery, GetMyRenewalFeeResponse>
{
    public async Task<GetMyRenewalFeeResponse> Handle(GetMyRenewalFeeQuery request, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.DoctorId);

        return new GetMyRenewalFeeResponse(doctor.RenewalFeeInRials);
    }
}
