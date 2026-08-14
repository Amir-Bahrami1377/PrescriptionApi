using FluentValidation;

namespace Prescription.Modules.Ticketing.Features.ReopenTicket;

public sealed class ReopenTicketValidator : AbstractValidator<ReopenTicketCommand>
{
    public ReopenTicketValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
