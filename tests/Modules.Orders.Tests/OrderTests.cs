using FluentAssertions;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Tests;

public class OrderTests
{
    private const long DoctorFee = 500_000;
    private const long ConsultationFee = 200_000;

    private static Order CreateOrder(bool requestsConsultation = false) =>
        Order.Create(
            Guid.NewGuid(),
            [Guid.NewGuid(), Guid.NewGuid()],
            customerNote: "note",
            customerUploadedFileKey: "file-key",
            basicInsurance: BasicInsuranceType.SocialSecurity,
            thirdParty: null,
            requestsConsultation: requestsConsultation);

    private static Order CreateClaimedOrder(Guid doctorId, bool requestsConsultation = false)
    {
        var order = CreateOrder(requestsConsultation);
        order.ClaimForReview(doctorId);
        return order;
    }

    private static Order CreateApprovedOrder(Guid doctorId, bool requestsConsultation = false)
    {
        var order = CreateClaimedOrder(doctorId, requestsConsultation);
        order.Approve(doctorId, DoctorFee, requestsConsultation ? ConsultationFee : null);
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

        var order = Order.Create(
            Guid.NewGuid(),
            [testId1, testId2],
            customerNote: null,
            customerUploadedFileKey: null,
            basicInsurance: BasicInsuranceType.None,
            thirdParty: null,
            requestsConsultation: false);

        order.LabTestIds.Should().BeEquivalentTo([testId1, testId2]);
    }

    [Fact]
    public void Create_WithDuplicateTestIds_Deduplicates()
    {
        var testId = Guid.NewGuid();

        var order = Order.Create(
            Guid.NewGuid(),
            [testId, testId],
            customerNote: null,
            customerUploadedFileKey: null,
            basicInsurance: BasicInsuranceType.None,
            thirdParty: null,
            requestsConsultation: false);

        order.LabTestIds.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithNoTestIds_Throws()
    {
        var act = () => Order.Create(
            Guid.NewGuid(),
            [],
            customerNote: null,
            customerUploadedFileKey: null,
            basicInsurance: BasicInsuranceType.None,
            thirdParty: null,
            requestsConsultation: false);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithoutAttachedFile_Succeeds()
    {
        var order = Order.Create(
            Guid.NewGuid(),
            [Guid.NewGuid()],
            customerNote: "note",
            customerUploadedFileKey: null,
            basicInsurance: BasicInsuranceType.None,
            thirdParty: null,
            requestsConsultation: false);

        order.CustomerUploadedFileKey.Should().BeNull();
        order.Status.Should().Be(OrderStatus.PendingDoctorApproval);
    }

    [Fact]
    public void Create_ForSelf_HasNoThirdPartyInfo()
    {
        var order = CreateOrder();

        order.IsForThirdParty.Should().BeFalse();
        order.ThirdPartyNationalCode.Should().BeNull();
        order.ThirdPartyPhoneNumber.Should().BeNull();
    }

    [Fact]
    public void Create_ForThirdParty_SetsThirdPartyInfo()
    {
        var order = Order.Create(
            Guid.NewGuid(),
            [Guid.NewGuid()],
            customerNote: null,
            customerUploadedFileKey: null,
            basicInsurance: BasicInsuranceType.SocialSecurity,
            thirdParty: new ThirdPartyBeneficiary("0499370899", "09121112233"),
            requestsConsultation: false);

        order.IsForThirdParty.Should().BeTrue();
        order.ThirdPartyNationalCode.Should().Be("0499370899");
        order.ThirdPartyPhoneNumber.Should().Be("09121112233");
        order.BasicInsurance.Should().Be(BasicInsuranceType.SocialSecurity);
    }

    [Theory]
    [InlineData("", "09121112233")]
    [InlineData("0499370899", "")]
    public void Create_ForThirdParty_MissingRequiredField_Throws(string nationalCode, string phoneNumber)
    {
        var act = () => Order.Create(
            Guid.NewGuid(),
            [Guid.NewGuid()],
            customerNote: null,
            customerUploadedFileKey: null,
            basicInsurance: BasicInsuranceType.None,
            thirdParty: new ThirdPartyBeneficiary(nationalCode, phoneNumber),
            requestsConsultation: false);

        act.Should().Throw<DomainException>();
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
        var order = CreateApprovedOrder(doctorId);

        var act = () => order.ClaimForReview(Guid.NewGuid());

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_WithoutClaiming_Throws()
    {
        var order = CreateOrder();

        var act = () => order.Approve(Guid.NewGuid(), DoctorFee, null);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_ClaimedBySomeoneElse_Throws()
    {
        var order = CreateOrder();
        order.ClaimForReview(Guid.NewGuid());

        var act = () => order.Approve(Guid.NewGuid(), DoctorFee, null);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_FromPendingDoctorApproval_TransitionsToAwaitingPayment_AndSetsDoctorAndPrice()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId);

        order.Approve(doctorId, DoctorFee, null);

        order.Status.Should().Be(OrderStatus.AwaitingPayment);
        order.DoctorId.Should().Be(doctorId);
        order.PriceInRials.Should().Be(DoctorFee);
        order.RequirePrice().Should().Be(DoctorFee);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId);

        var act = () => order.Approve(doctorId, DoctorFee, null);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_ConsultationRequested_AddsConsultationFeeToPrice()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId, requestsConsultation: true);

        order.Approve(doctorId, DoctorFee, ConsultationFee);

        order.PriceInRials.Should().Be(DoctorFee + ConsultationFee);
    }

    [Fact]
    public void Approve_ConsultationRequested_WithoutConsultationFee_Throws()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateClaimedOrder(doctorId, requestsConsultation: true);

        var act = () => order.Approve(doctorId, DoctorFee, null);

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
        var order = CreateApprovedOrder(doctorId);

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
        var order = CreateApprovedOrder(doctorId);
        order.RecordPaymentInitiated("authority-1");

        order.ConfirmPayment("ref-1");

        order.Status.Should().Be(OrderStatus.InProgress);
        order.PaymentReferenceId.Should().Be("ref-1");
    }

    [Fact]
    public void ConfirmPayment_ConsultationRequested_AlsoTransitionsToInProgress()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId, requestsConsultation: true);
        order.RecordPaymentInitiated("authority-1");

        order.ConfirmPayment("ref-1");

        // Consultation orders still pass through InProgress — that's where the doctor writes the
        // prescription and registers the tracking number the customer takes to the lab.
        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void UploadResult_WhenNotInProgress_Throws()
    {
        var order = CreateOrder();

        var act = () => order.UploadResult("result-key");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Complete_WithoutUploadedResult_StillCompletes()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");

        order.Complete();

        // A plain order needs no result file: the customer already has the prescription tracking
        // number and goes to the lab themselves.
        order.Status.Should().Be(OrderStatus.Completed);
        order.ResultFileKey.Should().BeNull();
    }

    [Fact]
    public void Complete_AfterResultUploaded_TransitionsToCompleted_AndSetsCompletedAtUtc()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");
        order.UploadResult("result-key");

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void UploadConsultationTestResult_WhenNotAwaitingTestResultUpload_Throws()
    {
        var order = CreateOrder(requestsConsultation: true);

        var act = () => order.UploadConsultationTestResult("result-key");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Complete_ConsultationRequested_HandsOffToTestResultUploadInsteadOfFinishing()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId, requestsConsultation: true);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");

        order.Complete();

        order.Status.Should().Be(OrderStatus.AwaitingTestResultUpload);
        order.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void UploadConsultationTestResult_FromAwaitingTestResultUpload_TransitionsToAwaitingConsultationOpinion()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId, requestsConsultation: true);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");
        order.Complete();

        order.UploadConsultationTestResult("result-key");

        order.Status.Should().Be(OrderStatus.AwaitingConsultationOpinion);
        order.ResultFileKey.Should().Be("result-key");
    }

    [Fact]
    public void SubmitConsultationOpinion_WhenNotAwaitingConsultationOpinion_Throws()
    {
        var order = CreateOrder(requestsConsultation: true);

        var act = () => order.SubmitConsultationOpinion("نظر پزشک");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void SubmitConsultationOpinion_FromAwaitingConsultationOpinion_CompletesOrder()
    {
        var doctorId = Guid.NewGuid();
        var order = CreateApprovedOrder(doctorId, requestsConsultation: true);
        order.RecordPaymentInitiated("authority-1");
        order.ConfirmPayment("ref-1");
        order.Complete();
        order.UploadConsultationTestResult("result-key");

        order.SubmitConsultationOpinion("نظر پزشک");

        order.Status.Should().Be(OrderStatus.Completed);
        order.ConsultationOpinion.Should().Be("نظر پزشک");
        order.CompletedAtUtc.Should().NotBeNull();
    }
}
