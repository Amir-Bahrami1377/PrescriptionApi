using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Infrastructure;

internal sealed class IdentityLookup(IdentityDbContext dbContext) : IIdentityLookup
{
    public async Task<DoctorFeeSnapshot?> GetDoctorFeeAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == doctorId && u.Role == UserRole.Doctor)
            .Select(u => new DoctorFeeSnapshot(u.Id, u.DoctorFeeInRials, u.ConsultationFeeInRials))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
