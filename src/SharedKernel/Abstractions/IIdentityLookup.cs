namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Read-only cross-module contract so Orders can resolve a doctor's fixed prescription fee at
/// approval time without taking a project reference on the Identity module — implemented by
/// Prescription.Modules.Identity.
/// </summary>
public interface IIdentityLookup
{
    Task<DoctorFeeSnapshot?> GetDoctorFeeAsync(Guid doctorId, CancellationToken cancellationToken = default);

    /// <summary>Checked against the database on every request rather than trusting a role claim, so an
    /// admin granting or revoking the capability takes effect immediately instead of after the
    /// patient's next login.</summary>
    Task<bool> IsSpecialPatientAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Null if the user doesn't exist or has been deactivated — callers should treat that as "nowhere to send an SMS" rather than an error.</summary>
    Task<string?> GetPhoneNumberAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Every active doctor's phone number, for broadcast notifications that aren't tied to one specific doctor (e.g. an unclaimed-order alert).</summary>
    Task<IReadOnlyList<string>> GetActiveDoctorPhoneNumbersAsync(CancellationToken cancellationToken = default);
}

/// <summary>A null fee means the doctor exists but hasn't configured that particular fee yet.</summary>
public sealed record DoctorFeeSnapshot(Guid DoctorId, long? FeeInRials, long? ConsultationFeeInRials, long? RenewalFeeInRials);
