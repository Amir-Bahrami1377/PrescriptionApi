using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Domain;

public sealed class Order : AuditableEntity
{
    private static readonly TimeSpan ClaimDuration = TimeSpan.FromMinutes(30);

    private readonly List<Guid> _labTestIds = [];

    private Order() { }

    public Guid CustomerId { get; private set; }
    public IReadOnlyCollection<Guid> LabTestIds => _labTestIds.AsReadOnly();

    /// <summary>Null until a doctor approves the order — the price is the approving doctor's fixed fee, not tied to the selected test(s).</summary>
    public long? PriceInRials { get; private set; }

    public string? CustomerNote { get; private set; }
    public string? CustomerUploadedFileKey { get; private set; }

    public BasicInsuranceType BasicInsurance { get; private set; }

    /// <summary>When true, the order was placed on someone else's behalf — that person is not required to be a registered user, this is order metadata only.</summary>
    public bool IsForThirdParty { get; private set; }
    public string? ThirdPartyNationalCode { get; private set; }
    public string? ThirdPartyPhoneNumber { get; private set; }

    /// <summary>When true, the approving doctor's fixed consultation fee is added on top of their visit fee, and after
    /// payment the order routes through customer self-upload + doctor opinion instead of finishing once the doctor is done.</summary>
    public bool RequestsConsultation { get; private set; }
    public string? ConsultationOpinion { get; private set; }

    public OrderStatus Status { get; private set; }

    public Guid? DoctorId { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? PrescriptionReferenceNumber { get; private set; }

    /// <summary>Exclusive 30-minute review window. Live-compared against UtcNow rather than
    /// proactively cleared by a background job — once ClaimExpiresAtUtc passes, the order simply
    /// falls back out of "claimed" queries and back into the shared pending pool on its own.</summary>
    public Guid? ClaimedByDoctorId { get; private set; }
    public DateTimeOffset? ClaimExpiresAtUtc { get; private set; }

    public string? PaymentAuthority { get; private set; }
    public string? PaymentReferenceId { get; private set; }

    public string? ResultFileKey { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static Order Create(
        Guid customerId,
        IEnumerable<Guid> labTestIds,
        string? customerNote,
        string? customerUploadedFileKey,
        BasicInsuranceType basicInsurance,
        ThirdPartyBeneficiary? thirdParty,
        bool requestsConsultation)
    {
        var distinctTestIds = labTestIds.Distinct().ToList();
        if (distinctTestIds.Count == 0)
        {
            throw new DomainException("انتخاب حداقل یک آزمایش الزامی است.");
        }

        if (thirdParty is not null
            && (string.IsNullOrWhiteSpace(thirdParty.NationalCode) || string.IsNullOrWhiteSpace(thirdParty.PhoneNumber)))
        {
            throw new DomainException("برای ثبت آزمایش برای فرد دیگر، کد ملی و شماره موبایل او الزامی است.");
        }

        var order = new Order
        {
            CustomerId = customerId,
            CustomerNote = customerNote,
            CustomerUploadedFileKey = customerUploadedFileKey,
            BasicInsurance = basicInsurance,
            IsForThirdParty = thirdParty is not null,
            ThirdPartyNationalCode = thirdParty?.NationalCode,
            ThirdPartyPhoneNumber = thirdParty?.PhoneNumber,
            RequestsConsultation = requestsConsultation,
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        order._labTestIds.AddRange(distinctTestIds);
        order.Submit();

        return order;
    }

    private void Submit()
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.PendingDoctorApproval);
        Status = OrderStatus.PendingDoctorApproval;
        Touch();
    }

    /// <summary>
    /// A doctor requests exclusive review rights. Fails if someone else already holds an
    /// unexpired claim; otherwise (free, expired, or already held by the same doctor) grants a
    /// fresh 30-minute window.
    /// </summary>
    public void ClaimForReview(Guid doctorId)
    {
        if (Status != OrderStatus.PendingDoctorApproval)
        {
            throw new ConflictException("این سفارش در وضعیت در انتظار بررسی نیست.");
        }

        var now = DateTimeOffset.UtcNow;
        var isClaimedBySomeoneElse = ClaimedByDoctorId is { } claimant
            && claimant != doctorId
            && ClaimExpiresAtUtc > now;

        if (isClaimedBySomeoneElse)
        {
            throw new ConflictException("این سفارش هم‌اکنون توسط پزشک دیگری در حال بررسی است.");
        }

        ClaimedByDoctorId = doctorId;
        ClaimExpiresAtUtc = now.Add(ClaimDuration);
        Touch();
    }

    public void Approve(Guid doctorId, long feeInRials, long? consultationFeeInRials)
    {
        EnsureActiveClaimBy(doctorId);
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.AwaitingPayment);

        if (RequestsConsultation && consultationFeeInRials is null)
        {
            throw new ConflictException("پزشک هنوز هزینه مشاوره خود را در پنل مدیریتی تعیین نکرده است.");
        }

        DoctorId = doctorId;
        PriceInRials = feeInRials + (RequestsConsultation ? consultationFeeInRials!.Value : 0);
        Status = OrderStatus.AwaitingPayment;
        Touch();
    }

    public void Reject(Guid doctorId, string reason)
    {
        EnsureActiveClaimBy(doctorId);
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.Rejected);
        DoctorId = doctorId;
        RejectionReason = reason;
        Status = OrderStatus.Rejected;
        Touch();
    }

    private void EnsureActiveClaimBy(Guid doctorId)
    {
        var hasActiveClaim = ClaimedByDoctorId == doctorId && ClaimExpiresAtUtc > DateTimeOffset.UtcNow;
        if (!hasActiveClaim)
        {
            throw new ConflictException("پیش از تایید یا رد سفارش، ابتدا باید درخواست بررسی برای آن ثبت کنید.");
        }
    }

    public void AttachPrescriptionReference(string prescriptionReferenceNumber)
    {
        if (DoctorId is null)
        {
            throw new ConflictException("پیش از ثبت ارجاع نسخه، سفارش باید توسط پزشک بررسی شده باشد.");
        }

        PrescriptionReferenceNumber = prescriptionReferenceNumber;
        Touch();
    }

    public void RecordPaymentInitiated(string authority)
    {
        if (Status != OrderStatus.AwaitingPayment)
        {
            throw new ConflictException("سفارش در وضعیت در انتظار پرداخت نیست.");
        }

        PaymentAuthority = authority;
        Touch();
    }

    /// <summary>Every paid order — consultation or not — enters InProgress, which is where the doctor
    /// writes the prescription and registers the tracking number the customer takes to the lab.</summary>
    public void ConfirmPayment(string referenceId)
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.InProgress);
        PaymentReferenceId = referenceId;
        Status = OrderStatus.InProgress;
        Touch();
    }

    public void UploadResult(string resultFileKey)
    {
        if (Status != OrderStatus.InProgress)
        {
            throw new ConflictException("جواب آزمایش فقط در وضعیت در حال انجام قابل بارگذاری است.");
        }

        ResultFileKey = resultFileKey;
        Touch();
    }

    /// <summary>
    /// The doctor signing off their part of the order. For a plain order that ends it — the customer
    /// already has the prescription tracking number and takes it to the lab themselves, so no result
    /// file is required here. For a consultation order this is instead the hand-off point: two steps
    /// remain (the customer uploads their result, then the doctor gives an opinion on it).
    /// </summary>
    public void Complete()
    {
        var targetStatus = RequestsConsultation ? OrderStatus.AwaitingTestResultUpload : OrderStatus.Completed;
        OrderStateMachine.EnsureCanTransition(Status, targetStatus);

        Status = targetStatus;
        if (targetStatus == OrderStatus.Completed)
        {
            CompletedAtUtc = DateTimeOffset.UtcNow;
        }

        Touch();
    }

    /// <summary>Consultation orders only: the customer uploads their own test result once it's ready.</summary>
    public void UploadConsultationTestResult(string resultFileKey)
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.AwaitingConsultationOpinion);
        ResultFileKey = resultFileKey;
        Status = OrderStatus.AwaitingConsultationOpinion;
        Touch();
    }

    /// <summary>Consultation orders only: the doctor's written opinion on the uploaded result completes the order.</summary>
    public void SubmitConsultationOpinion(string opinion)
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.Completed);
        ConsultationOpinion = opinion;
        Status = OrderStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>Safe accessor for callers that only run after Approve (AwaitingPayment onward), where PriceInRials is guaranteed set.</summary>
    public long RequirePrice() =>
        PriceInRials ?? throw new ConflictException("قیمت سفارش هنوز مشخص نشده است.");

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
