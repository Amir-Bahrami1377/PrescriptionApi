using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

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

    /// <summary>Fixed fee this doctor charges for reviewing/prescribing an order, set by the doctor themselves. Meaningful only when Role == Doctor.</summary>
    public long? DoctorFeeInRials { get; private set; }

    /// <summary>Fixed fee this doctor charges for giving a consultation opinion on an uploaded test result, set by the doctor themselves. Meaningful only when Role == Doctor.</summary>
    public long? ConsultationFeeInRials { get; private set; }

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

    public void ChangeRole(UserRole role)
    {
        Role = role;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetDoctorFee(long feeInRials)
    {
        if (Role != UserRole.Doctor)
        {
            throw new ConflictException("فقط پزشک می‌تواند هزینه ویزیت خود را تعیین کند.");
        }

        DoctorFeeInRials = feeInRials;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetConsultationFee(long feeInRials)
    {
        if (Role != UserRole.Doctor)
        {
            throw new ConflictException("فقط پزشک می‌تواند هزینه مشاوره خود را تعیین کند.");
        }

        ConsultationFeeInRials = feeInRials;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
