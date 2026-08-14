using FluentAssertions;
using Prescription.Modules.Ticketing.Domain;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Tests;

public sealed class TicketTests
{
    private static Ticket CreateTicket() =>
        Ticket.Create(Guid.NewGuid(), "موضوع تست", "پیام اولیه");

    [Fact]
    public void Create_StartsOpen_WithInitialCustomerMessage()
    {
        var ticket = CreateTicket();

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.Messages.Should().ContainSingle();
        ticket.Messages.Single().SenderRole.Should().Be("Customer");
    }

    [Fact]
    public void QueueForClosure_SetsSixHourDeadline()
    {
        var ticket = CreateTicket();
        var now = new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);

        ticket.QueueForClosure(now);

        ticket.Status.Should().Be(TicketStatus.PendingClosure);
        ticket.QueuedForClosureAtUtc.Should().Be(now);
        ticket.AutoCloseAtUtc.Should().Be(now.AddHours(6));
    }

    [Fact]
    public void CustomerReply_WhilePendingClosure_CancelsDeadlineAndOpensTicket()
    {
        var ticket = CreateTicket();
        ticket.QueueForClosure(DateTimeOffset.UtcNow);

        ticket.AddReply(ticket.CustomerId, "Customer", "هنوز سوال دارم");

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.QueuedForClosureAtUtc.Should().BeNull();
        ticket.AutoCloseAtUtc.Should().BeNull();
    }

    [Fact]
    public void AdminReply_WhilePendingClosure_DoesNotCancelDeadline()
    {
        var ticket = CreateTicket();
        var queuedAt = DateTimeOffset.UtcNow;
        ticket.QueueForClosure(queuedAt);

        ticket.AddReply(Guid.NewGuid(), "Admin", "توضیح تکمیلی");

        ticket.Status.Should().Be(TicketStatus.PendingClosure);
        ticket.AutoCloseAtUtc.Should().Be(queuedAt.AddHours(6));
    }

    [Fact]
    public void CloseIfClosureDeadlinePassed_BeforeDeadline_DoesNothing()
    {
        var ticket = CreateTicket();
        var queuedAt = DateTimeOffset.UtcNow;
        ticket.QueueForClosure(queuedAt);

        var closed = ticket.CloseIfClosureDeadlinePassed(queuedAt.AddHours(6).AddTicks(-1));

        closed.Should().BeFalse();
        ticket.Status.Should().Be(TicketStatus.PendingClosure);
    }

    [Fact]
    public void CloseIfClosureDeadlinePassed_AtDeadline_ClosesTicket()
    {
        var ticket = CreateTicket();
        var queuedAt = DateTimeOffset.UtcNow;
        var deadline = queuedAt.AddHours(6);
        ticket.QueueForClosure(queuedAt);

        var closed = ticket.CloseIfClosureDeadlinePassed(deadline);

        closed.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.ClosedAtUtc.Should().Be(deadline);
        ticket.AutoCloseAtUtc.Should().BeNull();
    }

    [Fact]
    public void CustomerCanClosePendingTicketImmediately()
    {
        var ticket = CreateTicket();
        ticket.QueueForClosure();

        ticket.CloseByCustomer();

        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.AutoCloseAtUtc.Should().BeNull();
    }

    [Fact]
    public void Reopen_ClosedTicket_ClearsClosedTimestamp()
    {
        var ticket = CreateTicket();
        ticket.CloseByCustomer();

        ticket.Reopen();

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.ClosedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ReplyToClosedTicket_Throws()
    {
        var ticket = CreateTicket();
        ticket.CloseByCustomer();

        var act = () => ticket.AddReply(ticket.CustomerId, "Customer", "پیام");

        act.Should().Throw<ConflictException>();
    }
}
