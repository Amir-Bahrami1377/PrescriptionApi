using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.ClaimOrderForReview;

public sealed class ClaimOrderForReviewHandler(OrdersDbContext dbContext)
    : IRequestHandler<ClaimOrderForReviewCommand, ClaimOrderForReviewResponse>
{
    public async Task<ClaimOrderForReviewResponse> Handle(ClaimOrderForReviewCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.ClaimForReview(request.DoctorId);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Two doctors claimed the same order in the same instant; the loser sees this instead
            // of silently overwriting the winner's claim.
            throw new ConflictException("این سفارش هم‌زمان توسط پزشک دیگری برداشته شد. لطفاً لیست را تازه‌سازی کنید.");
        }

        return new ClaimOrderForReviewResponse(order.ClaimExpiresAtUtc!.Value);
    }
}
