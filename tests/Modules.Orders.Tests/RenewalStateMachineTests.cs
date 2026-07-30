using FluentAssertions;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Tests;

public class RenewalStateMachineTests
{
    [Theory]
    [InlineData(RenewalStatus.PendingDoctorApproval, RenewalStatus.AwaitingPayment)]
    [InlineData(RenewalStatus.PendingDoctorApproval, RenewalStatus.Rejected)]
    [InlineData(RenewalStatus.AwaitingPayment, RenewalStatus.InProgress)]
    [InlineData(RenewalStatus.InProgress, RenewalStatus.Completed)]
    public void EnsureCanTransition_AllowedTransition_DoesNotThrow(RenewalStatus from, RenewalStatus to)
    {
        var act = () => RenewalStateMachine.EnsureCanTransition(from, to);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(RenewalStatus.PendingDoctorApproval, RenewalStatus.InProgress)]
    [InlineData(RenewalStatus.PendingDoctorApproval, RenewalStatus.Completed)]
    [InlineData(RenewalStatus.AwaitingPayment, RenewalStatus.Completed)]
    [InlineData(RenewalStatus.AwaitingPayment, RenewalStatus.Rejected)]
    [InlineData(RenewalStatus.InProgress, RenewalStatus.AwaitingPayment)]
    [InlineData(RenewalStatus.Completed, RenewalStatus.InProgress)]
    [InlineData(RenewalStatus.Rejected, RenewalStatus.PendingDoctorApproval)]
    public void EnsureCanTransition_DisallowedTransition_ThrowsConflictException(RenewalStatus from, RenewalStatus to)
    {
        var act = () => RenewalStateMachine.EnsureCanTransition(from, to);

        act.Should().Throw<ConflictException>();
    }
}
