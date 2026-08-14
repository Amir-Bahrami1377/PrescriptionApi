using FluentValidation;

namespace Prescription.Modules.Ticketing.Features.QueueTicketClosure;

public sealed class QueueTicketClosureValidator : AbstractValidator<QueueTicketClosureCommand>
{
    public QueueTicketClosureValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
