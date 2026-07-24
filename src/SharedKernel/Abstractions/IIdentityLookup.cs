namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Read-only cross-module contract so Orders can resolve a doctor's fixed prescription fee at
/// approval time without taking a project reference on the Identity module — implemented by
/// Prescription.Modules.Identity.
/// </summary>
public interface IIdentityLookup
{
    Task<DoctorFeeSnapshot?> GetDoctorFeeAsync(Guid doctorId, CancellationToken cancellationToken = default);
}

/// <summary>Null FeeInRials means the doctor exists but hasn't configured their fee yet.</summary>
public sealed record DoctorFeeSnapshot(Guid DoctorId, long? FeeInRials);
