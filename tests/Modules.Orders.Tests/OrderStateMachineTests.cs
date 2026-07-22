using FluentAssertions;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Tests;

public class OrderStateMachineTests
{
    [Theory]
    [InlineData(OrderStatus.Draft, OrderStatus.PendingDoctorApproval)]
    [InlineData(OrderStatus.PendingDoctorApproval, OrderStatus.AwaitingPayment)]
    [InlineData(OrderStatus.PendingDoctorApproval, OrderStatus.Rejected)]
    [InlineData(OrderStatus.AwaitingPayment, OrderStatus.InProgress)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Completed)]
    public void EnsureCanTransition_AllowedTransition_DoesNotThrow(OrderStatus from, OrderStatus to)
    {
        var act = () => OrderStateMachine.EnsureCanTransition(from, to);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(OrderStatus.Draft, OrderStatus.AwaitingPayment)]
    [InlineData(OrderStatus.Draft, OrderStatus.Completed)]
    [InlineData(OrderStatus.AwaitingPayment, OrderStatus.Completed)]
    [InlineData(OrderStatus.AwaitingPayment, OrderStatus.Rejected)]
    [InlineData(OrderStatus.InProgress, OrderStatus.AwaitingPayment)]
    [InlineData(OrderStatus.Completed, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Rejected, OrderStatus.PendingDoctorApproval)]
    public void EnsureCanTransition_DisallowedTransition_ThrowsConflictException(OrderStatus from, OrderStatus to)
    {
        var act = () => OrderStateMachine.EnsureCanTransition(from, to);

        act.Should().Throw<ConflictException>();
    }
}
