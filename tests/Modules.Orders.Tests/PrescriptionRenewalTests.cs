using FluentAssertions;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Tests;

public class PrescriptionRenewalTests
{
    private const long RenewalFee = 300_000;

    private static PrescriptionRenewal CreateRenewal() =>
        PrescriptionRenewal.Create(
            Guid.NewGuid(),
            currentPrescriptionReferenceNumber: "RX-OLD-1",
            nationalCode: "0499370899",
            basicInsurance: BasicInsuranceType.SocialSecurity);

    private static PrescriptionRenewal CreateClaimedRenewal(Guid doctorId)
    {
        var renewal = CreateRenewal();
        renewal.ClaimForReview(doctorId);
        return renewal;
    }

    private static PrescriptionRenewal CreatePaidRenewal(Guid doctorId)
    {
        var renewal = CreateClaimedRenewal(doctorId);
        renewal.Approve(doctorId, RenewalFee);
        renewal.RecordPaymentInitiated("authority-1");
        renewal.ConfirmPayment("ref-1");
        return renewal;
    }

    [Fact]
    public void Create_StartsPendingDoctorApproval_WithNoPriceYet()
    {
        var renewal = CreateRenewal();

        renewal.Status.Should().Be(RenewalStatus.PendingDoctorApproval);
        renewal.PriceInRials.Should().BeNull();
        renewal.NewPrescriptionReferenceNumber.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutCurrentTrackingCode_Throws(string reference)
    {
        var act = () => PrescriptionRenewal.Create(Guid.NewGuid(), reference, "0499370899", BasicInsuranceType.None);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutNationalCode_Throws(string nationalCode)
    {
        var act = () => PrescriptionRenewal.Create(Guid.NewGuid(), "RX-OLD-1", nationalCode, BasicInsuranceType.None);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void EnsureCustomerCanSubmitAnotherRenewal_UnderTheLimit_DoesNotThrow(int pendingCount)
    {
        var act = () => PrescriptionRenewal.EnsureCustomerCanSubmitAnotherRenewal(pendingCount);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void EnsureCustomerCanSubmitAnotherRenewal_AtOrOverTheLimit_Throws(int pendingCount)
    {
        var act = () => PrescriptionRenewal.EnsureCustomerCanSubmitAnotherRenewal(pendingCount);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ClaimForReview_FreshRequest_GrantsThirtyMinuteWindow()
    {
        var renewal = CreateRenewal();
        var doctorId = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow;

        renewal.ClaimForReview(doctorId);

        renewal.ClaimedByDoctorId.Should().Be(doctorId);
        renewal.ClaimExpiresAtUtc.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClaimForReview_AlreadyClaimedBySomeoneElse_Throws()
    {
        var renewal = CreateRenewal();
        renewal.ClaimForReview(Guid.NewGuid());

        var act = () => renewal.ClaimForReview(Guid.NewGuid());

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_WithoutClaiming_Throws()
    {
        var renewal = CreateRenewal();

        var act = () => renewal.Approve(Guid.NewGuid(), RenewalFee);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Approve_SetsDoctorAndTariff_AndMovesToAwaitingPayment()
    {
        var doctorId = Guid.NewGuid();
        var renewal = CreateClaimedRenewal(doctorId);

        renewal.Approve(doctorId, RenewalFee);

        renewal.Status.Should().Be(RenewalStatus.AwaitingPayment);
        renewal.DoctorId.Should().Be(doctorId);
        renewal.PriceInRials.Should().Be(RenewalFee);
    }

    [Fact]
    public void Reject_WithoutClaiming_Throws()
    {
        var renewal = CreateRenewal();

        var act = () => renewal.Reject(Guid.NewGuid(), "دلیل");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Reject_SetsReasonAndMovesToRejected()
    {
        var doctorId = Guid.NewGuid();
        var renewal = CreateClaimedRenewal(doctorId);

        renewal.Reject(doctorId, "نسخه قابل تمدید نیست.");

        renewal.Status.Should().Be(RenewalStatus.Rejected);
        renewal.RejectionReason.Should().Be("نسخه قابل تمدید نیست.");
    }

    [Fact]
    public void RecordPaymentInitiated_BeforeApproval_Throws()
    {
        var renewal = CreateRenewal();

        var act = () => renewal.RecordPaymentInitiated("authority-1");

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ConfirmPayment_MovesToInProgress()
    {
        var doctorId = Guid.NewGuid();
        var renewal = CreatePaidRenewal(doctorId);

        renewal.Status.Should().Be(RenewalStatus.InProgress);
        renewal.PaymentReferenceId.Should().Be("ref-1");
    }

    [Fact]
    public void Complete_BeforePayment_Throws()
    {
        var doctorId = Guid.NewGuid();
        var renewal = CreateClaimedRenewal(doctorId);
        renewal.Approve(doctorId, RenewalFee);

        var act = () => renewal.Complete("RX-NEW-1");

        act.Should().Throw<ConflictException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Complete_WithoutNewTrackingCode_Throws(string newReference)
    {
        var renewal = CreatePaidRenewal(Guid.NewGuid());

        var act = () => renewal.Complete(newReference);

        // A completed renewal with no number to hand back would be useless to the patient.
        act.Should().Throw<DomainException>();
        renewal.Status.Should().Be(RenewalStatus.InProgress);
    }

    [Fact]
    public void Complete_WithNewTrackingCode_FinishesAndHandsItBack()
    {
        var renewal = CreatePaidRenewal(Guid.NewGuid());

        renewal.Complete("RX-NEW-1");

        renewal.Status.Should().Be(RenewalStatus.Completed);
        renewal.NewPrescriptionReferenceNumber.Should().Be("RX-NEW-1");
        renewal.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void RequirePrice_BeforeApproval_Throws()
    {
        var renewal = CreateRenewal();

        var act = renewal.RequirePrice;

        act.Should().Throw<ConflictException>();
    }
}
