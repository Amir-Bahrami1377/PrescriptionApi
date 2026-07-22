using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Domain;

public sealed class Order : AuditableEntity
{
    private Order() { }

    public Guid CustomerId { get; private set; }
    public Guid LabTestId { get; private set; }
    public long PriceInRials { get; private set; }
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

    public static Order Create(Guid customerId, Guid labTestId, long priceInRials, string? customerNote, string? customerUploadedFileKey)
    {
        var order = new Order
        {
            CustomerId = customerId,
            LabTestId = labTestId,
            PriceInRials = priceInRials,
            CustomerNote = customerNote,
            CustomerUploadedFileKey = customerUploadedFileKey,
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        order.Submit();

        return order;
    }

    private void Submit()
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.PendingDoctorApproval);
        Status = OrderStatus.PendingDoctorApproval;
        Touch();
    }

    public void Approve(Guid doctorId)
    {
        OrderStateMachine.EnsureCanTransition(Status, OrderStatus.AwaitingPayment);
        DoctorId = doctorId;
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

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
