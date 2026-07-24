using FluentAssertions;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Tests;

public class OrderTests
{
    private const long DoctorFee = 500_000;

    private static Order CreateOrder() =>
        Order.Create(Guid.NewGuid(), Guid.NewGuid(), customerNote: "note", customerUploadedFileKey: "file-key");

    [Fact]
    public void Create_NewOrder_StartsInPendingDoctorApproval()
    {
        var order = CreateOrder();

        order.Status.Should().Be(OrderStatus.PendingDoctorApproval);
    }

    [Fact]
    public void Create_NewOrder_HasNoPriceYet()
    {
        var order = CreateOrder();

        order.PriceInRials.Should().BeNull();
    }

    [Fact]
    public void RequirePrice_BeforeApproval_Throws()
    {
        var order = CreateOrder();

        var act = order.RequirePrice;

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_FromPendingDoctorApproval_TransitionsToAwaitingPayment_AndSetsDoctorAndPrice()
    {
        var order = CreateOrder();
        var doctorId = Guid.NewGuid();

        order.Approve(doctorId, DoctorFee);

        order.Status.Should().Be(OrderStatus.AwaitingPayment);
        order.DoctorId.Should().Be(doctorId);
        order.PriceInRials.Should().Be(DoctorFee);
        order.RequirePrice().Should().Be(DoctorFee);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var order = CreateOrder();
        order.Approve(Guid.NewGuid(), DoctorFee);

        var act = () => order.Approve(Guid.NewGuid(), DoctorFee);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Reject_FromPendingDoctorApproval_TransitionsToRejected_AndSetsReason()
    {
        var order = CreateOrder();
        var doctorId = Guid.NewGuid();

        order.Reject(doctorId, "کیفیت تصویر کافی نیست.");

        order.Status.Should().Be(OrderStatus.Rejected);
        order.DoctorId.Should().Be(doctorId);
        order.RejectionReason.Should().Be("کیفیت تصویر کافی نیست.");
    }

    [Fact]
    public void AttachPrescriptionReference_BeforeDoctorReview_Throws()
    {
        var order = CreateOrder();

        var act = () => order.AttachPrescriptionReference("REF-123");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void AttachPrescriptionReference_AfterApproval_Succeeds()
    {
        var order = CreateOrder();
        order.Approve(Guid.NewGuid(), DoctorFee);

        order.AttachPrescriptionReference("REF-123");

        order.PrescriptionReferenceNumber.Should().Be("REF-123");
    }

    [Fact]
    public void RecordPaymentInitiated_WhenNotAwaitingPayment_Throws()
    {
        var order = CreateOrder();

        var act = () => order.RecordPaymentInitiated("authority-1");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ConfirmPayment_FromAwaitingPayment_TransitionsToInProgress()
    {
        var order = CreateOrder();
        order.Approve(Guid.NewGuid(), DoctorFee);
        order.RecordPaymentInitiated("authority-1");

        order.ConfirmPayment("ref-1");

        order.Status.Should().Be(OrderStatus.InProgress);
        order.PaymentReferenceId.Should().Be("ref-1");
    }

    [Fact]
    public void UploadResult_WhenNotInProgress_Throws()
    {
        var order = CreateOrder();

        var act = () => order.UploadResult("result-key");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Complete_WithoutUploadedResult_Throws()
    {
        var order = CreateOrder();
        order.Approve(Guid.NewGuid(), DoctorFee);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");

        var act = () => order.Complete();

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Complete_AfterResultUploaded_TransitionsToCompleted_AndSetsCompletedAtUtc()
    {
        var order = CreateOrder();
        order.Approve(Guid.NewGuid(), DoctorFee);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");
        order.UploadResult("result-key");

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAtUtc.Should().NotBeNull();
    }
}
