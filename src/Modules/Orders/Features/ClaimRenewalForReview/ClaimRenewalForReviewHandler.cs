using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.ClaimRenewalForReview;

public sealed class ClaimRenewalForReviewHandler(OrdersDbContext dbContext)
    : IRequestHandler<ClaimRenewalForReviewCommand, ClaimRenewalForReviewResponse>
{
    public async Task<ClaimRenewalForReviewResponse> Handle(ClaimRenewalForReviewCommand request, CancellationToken cancellationToken)
    {
        var renewal = await dbContext.PrescriptionRenewals.FirstOrDefaultAsync(r => r.Id == request.RenewalId, cancellationToken)
            ?? throw new NotFoundException(nameof(PrescriptionRenewal), request.RenewalId);

        renewal.ClaimForReview(request.DoctorId);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Two doctors claimed the same request in the same instant; the loser is told rather than
            // silently overwriting the winner's claim.
            throw new ConflictException("این درخواست هم‌زمان توسط پزشک دیگری برداشته شد. لطفاً لیست را تازه‌سازی کنید.");
        }

        return new ClaimRenewalForReviewResponse(renewal.ClaimExpiresAtUtc!.Value);
    }
}
