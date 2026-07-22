using FluentValidation;

namespace Prescription.Modules.Ticketing.Features.ReplyTicket;

public sealed class ReplyTicketValidator : AbstractValidator<ReplyTicketCommand>
{
    public ReplyTicketValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.SenderRole).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
