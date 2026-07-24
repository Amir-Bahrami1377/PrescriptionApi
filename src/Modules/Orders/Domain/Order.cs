using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Domain;

public sealed class Order : AuditableEntity
{
    private readonly List<Guid> _labTestIds = [];

    private Order() { }

    public Guid CustomerId { get; private set; }
    public IReadOnlyCollection<Guid> LabTestIds => _labTestIds.AsReadOnly();

    /// <summary>Null until a doctor approves the order — the price is the approving doctor's fixed fee, not tied to the selected test(s).</summary>
    public long? PriceInRials { get; private set; }

    public string? CustomerNote { get; private set; }
    public string? CustomerUploadedFileKey { get; private set; }

    public OrderStatus Status { get; private set; }

    public Guid? DoctorId { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? PrescriptionReferenceNumber { get; private set; }

    public string? PaymentAuthority { get; private set; }
    public string? PaymentReferenceId { get; private set; }

    public string? ResultFileKey { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static Order Create(Guid customerId, IEnumerable<Guid> labTestIds, string? customerNote, string? customerUploadedFileKey)
    {
        var distinctTestIds = labTestIds.Distinct().ToList();
        if (distinctTestIds.Count == 0)
        {
            throw new DomainException("انتخاب حداقل یک آزمایش الزامی است.");
        }

        var order = new Order
        {
            CustomerId = customerId,
            CustomerNote = customerNote,
            CustomerUploadedFileKey = customerUploadedFileKey,
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

    public void Approve(Guid doctorId, long feeInRials)
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.AwaitingPayment);
        DoctorId = doctorId;
        PriceInRials = feeInRials;
        Status = OrderStatus.AwaitingPayment;
        Touch();
    }

    public void Reject(Guid doctorId, string reason)
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.Rejected);
        DoctorId = doctorId;
        RejectionReason = reason;
        Status = OrderStatus.Rejected;
        Touch();
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

    public void Complete()
    {
        if (ResultFileKey is null)
        {
            throw new ConflictException("پیش از تکمیل سفارش باید جواب آزمایش بارگذاری شود.");
        }

        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.Completed);
        Status = OrderStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>Safe accessor for callers that only run after Approve (AwaitingPayment onward), where PriceInRials is guaranteed set.</summary>
    public long RequirePrice() =>
        PriceInRials ?? throw new ConflictException("قیمت سفارش هنوز مشخص نشده است.");

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
