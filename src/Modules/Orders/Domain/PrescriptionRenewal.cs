using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Domain;

/// <summary>
/// A special patient asking for an existing prescription to be reissued. Deliberately its own
/// aggregate rather than a flavour of Order: there are no lab tests, no result file and no
/// consultation branch — just the tracking number of the prescription being renewed, and the new one
/// the doctor hands back at the end.
/// </summary>
public sealed class PrescriptionRenewal : AuditableEntity
{
    private static readonly TimeSpan ClaimDuration = TimeSpan.FromMinutes(30);

    private PrescriptionRenewal() { }

    /// <summary>Matches the lab-order cap so a patient can't flood the doctors' shared review pool.
    /// Counted separately from lab orders, so three of each may be open at once.</summary>
    public const int MaxPendingApprovalPerCustomer = 3;

    public Guid CustomerId { get; private set; }

    /// <summary>Tracking number of the prescription the patient wants renewed.</summary>
    public string CurrentPrescriptionReferenceNumber { get; private set; } = null!;

    /// <summary>Encrypted at rest, same as every other national code in the system.</summary>
    public string NationalCode { get; private set; } = null!;

    public BasicInsuranceType BasicInsurance { get; private set; }

    public RenewalStatus Status { get; private set; }

    /// <summary>Null until a doctor approves — the price is the approving doctor's renewal tariff.</summary>
    public long? PriceInRials { get; private set; }

    public Guid? DoctorId { get; private set; }
    public string? RejectionReason { get; private set; }

    /// <summary>Same live-compared 30-minute exclusivity as orders: once it lapses the request drops
    /// back into the shared pool on its own, with no background job.</summary>
    public Guid? ClaimedByDoctorId { get; private set; }
    public DateTimeOffset? ClaimExpiresAtUtc { get; private set; }

    public string? PaymentAuthority { get; private set; }
    public string? PaymentReferenceId { get; private set; }

    /// <summary>The reissued prescription's tracking number, handed back when the doctor finishes.</summary>
    public string? NewPrescriptionReferenceNumber { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static void EnsureCustomerCanSubmitAnotherRenewal(int pendingApprovalCount)
    {
        if (pendingApprovalCount >= MaxPendingApprovalPerCustomer)
        {
            throw new DomainException(
                $"در هر زمان حداکثر می‌توانید {MaxPendingApprovalPerCustomer} درخواست تمدید در انتظار بررسی پزشک داشته باشید. لطفاً تا بررسی درخواست‌های قبلی صبر کنید.");
        }
    }

    public static PrescriptionRenewal Create(
        Guid customerId,
        string currentPrescriptionReferenceNumber,
        string nationalCode,
        BasicInsuranceType basicInsurance)
    {
        if (string.IsNullOrWhiteSpace(currentPrescriptionReferenceNumber))
        {
            throw new DomainException("ثبت کد رهگیری نسخه الزامی است.");
        }

        if (string.IsNullOrWhiteSpace(nationalCode))
        {
            throw new DomainException("ثبت کد ملی الزامی است.");
        }

        return new PrescriptionRenewal
        {
            CustomerId = customerId,
            CurrentPrescriptionReferenceNumber = currentPrescriptionReferenceNumber,
            NationalCode = nationalCode,
            BasicInsurance = basicInsurance,
            Status = RenewalStatus.PendingDoctorApproval,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void ClaimForReview(Guid doctorId)
    {
        if (Status != RenewalStatus.PendingDoctorApproval)
        {
            throw new ConflictException("این درخواست در وضعیت در انتظار بررسی نیست.");
        }

        var now = DateTimeOffset.UtcNow;
        var isClaimedBySomeoneElse = ClaimedByDoctorId is { } claimant
            && claimant != doctorId
            && ClaimExpiresAtUtc > now;

        if (isClaimedBySomeoneElse)
        {
            throw new ConflictException("این درخواست هم‌اکنون توسط پزشک دیگری در حال بررسی است.");
        }

        ClaimedByDoctorId = doctorId;
        ClaimExpiresAtUtc = now.Add(ClaimDuration);
        Touch();
    }

    public void Approve(Guid doctorId, long feeInRials)
    {
        EnsureActiveClaimBy(doctorId);
        RenewalStateMachine.EnsureCanTransition(Status, RenewalStatus.AwaitingPayment);
        DoctorId = doctorId;
        PriceInRials = feeInRials;
        Status = RenewalStatus.AwaitingPayment;
        Touch();
    }

    public void Reject(Guid doctorId, string reason)
    {
        EnsureActiveClaimBy(doctorId);
        RenewalStateMachine.EnsureCanTransition(Status, RenewalStatus.Rejected);
        DoctorId = doctorId;
        RejectionReason = reason;
        Status = RenewalStatus.Rejected;
        Touch();
    }

    private void EnsureActiveClaimBy(Guid doctorId)
    {
        var hasActiveClaim = ClaimedByDoctorId == doctorId && ClaimExpiresAtUtc > DateTimeOffset.UtcNow;
        if (!hasActiveClaim)
        {
            throw new ConflictException("پیش از تایید یا رد درخواست، ابتدا باید درخواست بررسی برای آن ثبت کنید.");
        }
    }

    public void RecordPaymentInitiated(string authority)
    {
        if (Status != RenewalStatus.AwaitingPayment)
        {
            throw new ConflictException("درخواست در وضعیت در انتظار پرداخت نیست.");
        }

        PaymentAuthority = authority;
        Touch();
    }

    public void ConfirmPayment(string referenceId)
    {
        RenewalStateMachine.EnsureCanTransition(Status, RenewalStatus.InProgress);
        PaymentReferenceId = referenceId;
        Status = RenewalStatus.InProgress;
        Touch();
    }

    /// <summary>Finishing and handing back the new tracking number are one step on purpose — a completed
    /// renewal with no number to show the patient would be worthless to them.</summary>
    public void Complete(string newPrescriptionReferenceNumber)
    {
        if (string.IsNullOrWhiteSpace(newPrescriptionReferenceNumber))
        {
            throw new DomainException("برای تکمیل درخواست، ثبت کد رهگیری نسخه جدید الزامی است.");
        }

        RenewalStateMachine.EnsureCanTransition(Status, RenewalStatus.Completed);
        NewPrescriptionReferenceNumber = newPrescriptionReferenceNumber;
        Status = RenewalStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    public long RequirePrice() =>
        PriceInRials ?? throw new ConflictException("مبلغ درخواست هنوز مشخص نشده است.");

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
