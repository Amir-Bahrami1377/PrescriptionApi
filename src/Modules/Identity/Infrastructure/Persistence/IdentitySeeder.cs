using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Infrastructure.Persistence;

/// <summary>Dev/local-only seeding, invoked from Program.cs alongside the auto-migration step.</summary>
public static class IdentitySeeder
{
    public static async Task SeedAdminAsync(IdentityDbContext dbContext, string adminPhoneNumber, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = PhoneNumberNormalizer.Normalize(adminPhoneNumber);

        var existing = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var admin = User.RegisterFromPhoneNumber(normalizedPhone, UserRole.Admin);
        admin.CompleteProfile(nationalCode: "0499370899", fullName: "مدیر سیستم", age: 30, gender: Gender.Male);

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
