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

        user.DoctorFeeInRials.Should().Be(500_000);
        user.ConsultationFeeInRials.Should().Be(150_000);
    }
}
