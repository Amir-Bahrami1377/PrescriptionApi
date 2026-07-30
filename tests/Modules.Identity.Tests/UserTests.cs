using FluentAssertions;
using Prescription.Modules.Identity.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Identity.Tests;

public class UserTests
{
    [Fact]
    public void RegisterFromPhoneNumber_NewUser_IsActive()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_MarksUserInactive()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ChangeRole_OnDeactivatedUser_ReactivatesThem()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");
        user.Deactivate();

        user.ChangeRole(UserRole.Doctor);

        user.IsActive.Should().BeTrue();
        user.Role.Should().Be(UserRole.Doctor);
    }

    [Fact]
    public void SetDoctorFee_WhenNotDoctor_Throws()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        var act = () => user.SetDoctorFee(500_000);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void SetConsultationFee_WhenNotDoctor_Throws()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        var act = () => user.SetConsultationFee(150_000);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void SetFees_WhenDoctor_Succeeds()
    {
        var user = User.RegisterFromPhoneNumber("09121112233", UserRole.Doctor);

        user.SetDoctorFee(500_000);
        user.SetConsultationFee(150_000);
        user.SetRenewalFee(300_000);

        user.DoctorFeeInRials.Should().Be(500_000);
        user.ConsultationFeeInRials.Should().Be(150_000);
        user.RenewalFeeInRials.Should().Be(300_000);
    }

    [Fact]
    public void SetRenewalFee_WhenNotDoctor_Throws()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        var act = () => user.SetRenewalFee(300_000);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void RegisterFromPhoneNumber_NewUser_IsNotSpecialPatient()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        user.IsSpecialPatient.Should().BeFalse();
    }

    [Fact]
    public void SetSpecialPatient_OnCustomer_GrantsAndRevokes()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");

        user.SetSpecialPatient(true);
        user.IsSpecialPatient.Should().BeTrue();

        user.SetSpecialPatient(false);
        user.IsSpecialPatient.Should().BeFalse();
    }

    [Fact]
    public void SetSpecialPatient_OnStaff_Throws()
    {
        var doctor = User.RegisterFromPhoneNumber("09121112233", UserRole.Doctor);

        var act = () => doctor.SetSpecialPatient(true);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ChangeRole_AwayFromCustomer_DropsSpecialPatient()
    {
        var user = User.RegisterFromPhoneNumber("09121112233");
        user.SetSpecialPatient(true);

        user.ChangeRole(UserRole.Doctor);

        // Otherwise the account would sit as a "special patient doctor", which means nothing.
        user.IsSpecialPatient.Should().BeFalse();
    }
}
