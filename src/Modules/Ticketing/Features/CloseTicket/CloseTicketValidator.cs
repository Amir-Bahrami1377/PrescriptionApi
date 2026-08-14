using FluentValidation;

namespace Prescription.Modules.Ticketing.Features.CloseTicket;

public sealed class CloseTicketValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
