using FluentAssertions;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Tests;

public class OrderTests
{
    private const long DoctorFee = 500_000;

    private static Order CreateOrder() =>
        Order.Create(Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()], customerNote: "note", customerUploadedFileKey: "file-key");

    private static Order CreateClaimedOrder(Guid doctorId)
    {
        var order = CreateOrder();
        order.ClaimForReview(doctorId);
        return order;
    }

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
    public void Create_WithMultipleTests_KeepsAllOfThem()
    {
        var testId1 = Guid.NewGuid();
        var testId2 = Guid.NewGuid();

        var order = Order.Create(Guid.NewGuid(), [testId1, testId2], customerNote: null, customerUploadedFileKey: null);

        order.LabTestIds.Should().BeEquivalentTo([testId1, testId2]);
    }

    [Fact]
    public void Create_WithDuplicateTestIds_Deduplicates()
    {
        var testId = Guid.NewGuid();

        var order = Order.Create(Guid.NewGuid(), [testId, testId], customerNote: null, customerUploadedFileKey: null);

        order.LabTestIds.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithNoTestIds_Throws()
    {
        var act = () => Order.Create(Guid.NewGuid(), [], customerNote: null, customerUploadedFileKey: null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithoutAttachedFile_Succeeds()
    {
        var order = Order.Create(Guid.NewGuid(), [Guid.NewGuid()], customerNote: "note", customerUploadedFileKey: null);

        order.CustomerUploadedFileKey.Should().BeNull();
        order.Status.Should().Be(OrderStatus.PendingDoctorApproval);
    }

    [Fact]
    public void RequirePrice_BeforeApproval_Throws()
    {
        var order = CreateOrder();

        var act = order.RequirePrice;

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ClaimForReview_FreshOrder_GrantsThirtyMinuteWindow()
    {
        var order = CreateOrder();
        var doctorId = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow;

        order.ClaimForReview(doctorId);

        order.ClaimedByDoctorId.Should().Be(doctorId);
        order.ClaimExpiresAtUtc.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClaimForReview_AlreadyClaimedBySomeoneElse_Throws()
    {
        var order = CreateOrder();
        order.ClaimForReview(Guid.NewGuid());

        var act = () => order.ClaimForReview(Guid.NewGuid());

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ClaimForReview_ReclaimedBySameDoctor_RefreshesWindow()
    {
        var order = CreateOrder();
        var doctorId = Guid.NewGuid();
        order.ClaimForReview(doctorId);

        var act = () => order.ClaimForReview(doctorId);

        act.Should().NotThrow();
        order.ClaimedByDoctorId.Should().Be(doctorId);
    }

    [Fact]
    public void ClaimForReview_NotPendingDoctorApproval_Throws()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);
        order.Approve(doctorId, DoctorFee);

        var act = () => order.ClaimForReview(Guid.NewGuid());

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_WithoutClaiming_Throws()
    {
        var order = CreateOrder();

        var act = () => order.Approve(Guid.NewGuid(), DoctorFee);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_ClaimedBySomeoneElse_Throws()
    {
        var order = CreateOrder();
        order.ClaimForReview(Guid.NewGuid());

        var act = () => order.Approve(Guid.NewGuid(), DoctorFee);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_FromPendingDoctorApproval_TransitionsToAwaitingPayment_AndSetsDoctorAndPrice()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);

        order.Approve(doctorId, DoctorFee);

        order.Status.Should().Be(OrderStatus.AwaitingPayment);
        order.DoctorId.Should().Be(doctorId);
        order.PriceInRials.Should().Be(DoctorFee);
        order.RequirePrice().Should().Be(DoctorFee);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);
        order.Approve(doctorId, DoctorFee);

        var act = () => order.Approve(doctorId, DoctorFee);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Reject_WithoutClaiming_Throws()
    {
        var order = CreateOrder();

        var act = () => order.Reject(Guid.NewGuid(), "دلیل");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Reject_FromPendingDoctorApproval_TransitionsToRejected_AndSetsReason()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);

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
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);
        order.Approve(doctorId, DoctorFee);

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
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);
        order.Approve(doctorId, DoctorFee);
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
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);
        order.Approve(doctorId, DoctorFee);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");

        var act = () => order.Complete();

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Complete_AfterResultUploaded_TransitionsToCompleted_AndSetsCompletedAtUtc()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);
        order.Approve(doctorId, DoctorFee);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");
        order.UploadResult("result-key");

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAtUtc.Should().NotBeNull();
    }
}
