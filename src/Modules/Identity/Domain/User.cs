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

    /// <summary>False once an admin deletes the user. Soft delete, because Orders references users by
    /// bare Guid with no FK (CustomerId/DoctorId), so removing the row would orphan order history.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Fixed fee this doctor charges for reviewing/prescribing an order, set by the doctor themselves. Meaningful only when Role == Doctor.</summary>
    public long? DoctorFeeInRials { get; private set; }

    /// <summary>Fixed fee this doctor charges for giving a consultation opinion on an uploaded test result, set by the doctor themselves. Meaningful only when Role == Doctor.</summary>
    public long? ConsultationFeeInRials { get; private set; }

    /// <summary>Fixed tariff this doctor charges for renewing a prescription, set by the doctor themselves. Meaningful only when Role == Doctor.</summary>
    public long? RenewalFeeInRials { get; private set; }

    /// <summary>Prescription renewal is invite-only: a capability an admin grants on top of the normal
    /// Customer role rather than a role of its own, so these patients keep every ability an ordinary
    /// customer has (placing lab orders, paying, uploading results).</summary>
    public bool IsSpecialPatient { get; private set; }

    public static User RegisterFromPhoneNumber(string phoneNumber, UserRole role = UserRole.Customer)
    {
        return new User
        {
            PhoneNumber = phoneNumber,
            Role = role,
            IsProfileCompleted = false,
            IsActive = true,
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
        // Re-adding a previously deleted phone number through the admin panel brings the account back
        // rather than silently leaving it locked out.
        IsActive = true;

        // Special patient is a customer-only capability; promoting someone to staff drops it so the
        // account can't sit in a half-meaningful state.
        if (role != UserRole.Customer)
        {
            IsSpecialPatient = false;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetSpecialPatient(bool isSpecialPatient)
    {
        if (isSpecialPatient && Role != UserRole.Customer)
        {
            throw new ConflictException("نقش بیمار ویژه فقط برای کاربران عادی (بیمار) قابل تعریف است.");
        }

        IsSpecialPatient = isSpecialPatient;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
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

    public void SetRenewalFee(long feeInRials)
    {
        if (Role != UserRole.Doctor)
        {
            throw new ConflictException("فقط پزشک می‌تواند تعرفه تمدید نسخه خود را تعیین کند.");
        }

        RenewalFeeInRials = feeInRials;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
