using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Infrastructure.Persistence;

/// <summary>Bootstraps the account that can reach the admin panel, invoked from Program.cs alongside
/// the migration step. Idempotent, so it is safe on every startup.</summary>
public static class IdentitySeeder
{
    /// <param name="usePlaceholderProfile">
    /// Fills the profile with stand-in details so local development doesn't have to complete it by
    /// hand. Off for real deployments, where the administrator should enter their own information on
    /// first login instead of carrying a fabricated national code on their account.
    /// </param>
    public static async Task SeedAdminAsync(
        IdentityDbContext dbContext,
        string adminPhoneNumber,
        bool usePlaceholderProfile,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = PhoneNumberNormalizer.Normalize(adminPhoneNumber);

        var existing = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone, cancellationToken);
        if (existing is not null)
        {
            // The number is very often already registered as an ordinary customer — whoever runs the
            // site tends to have signed in as a patient first. Returning here would leave nobody able
            // to open the admin panel, so promote instead of silently doing nothing.
            if (existing.Role != UserRole.Admin)
            {
                existing.ChangeRole(UserRole.Admin);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var admin = User.RegisterFromPhoneNumber(normalizedPhone, UserRole.Admin);

        if (usePlaceholderProfile)
        {
            admin.CompleteProfile(nationalCode: "0499370899", fullName: "مدیر سیستم", age: 30, gender: Gender.Male);
        }

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
