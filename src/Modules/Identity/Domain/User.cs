using Prescription.SharedKernel.Entities;

namespace Prescription.Modules.Identity.Domain;

public sealed class User : AuditableEntity
{
    private User() { }

    public string PhoneNumber { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public string? NationalCode { get; private set; }
    public string? FullName { get; private set; }
    public int? Age { get; private set; }
    public Gender? Gender { get; private set; }
    public bool IsProfileCompleted { get; private set; }

    public static User RegisterFromPhoneNumber(string phoneNumber, UserRole role = UserRole.Customer)
    {
        return new User
        {
            PhoneNumber = phoneNumber,
            Role = role,
            IsProfileCompleted = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void CompleteProfile(string nationalCode, string fullName, int age, Gender gender)
    {
        NationalCode = nationalCode;
        FullName = fullName;
        Age = age;
        Gender = gender;
        IsProfileCompleted = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
