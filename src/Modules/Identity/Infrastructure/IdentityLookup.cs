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
            .Select(u => new DoctorFeeSnapshot(u.Id, u.DoctorFeeInRials, u.ConsultationFeeInRials, u.RenewalFeeInRials))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsSpecialPatientAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.IsActive && u.IsSpecialPatient, cancellationToken);
    }

    public async Task<string?> GetPhoneNumberAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => u.PhoneNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetActiveDoctorPhoneNumbersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Doctor && u.IsActive)
            .Select(u => u.PhoneNumber)
            .ToListAsync(cancellationToken);
    }
}
