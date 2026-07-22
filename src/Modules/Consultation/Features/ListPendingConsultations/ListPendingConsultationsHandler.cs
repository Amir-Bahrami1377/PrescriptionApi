using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Consultation.Domain;
using Prescription.Modules.Consultation.Infrastructure.Persistence;

namespace Prescription.Modules.Consultation.Features.ListPendingConsultations;

public sealed class ListPendingConsultationsHandler(ConsultationDbContext dbContext)
    : IRequestHandler<ListPendingConsultationsQuery, IReadOnlyList<PendingConsultationDto>>
{
    public async Task<IReadOnlyList<PendingConsultationDto>> Handle(ListPendingConsultationsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.ConsultationRequests
            .AsNoTracking()
            .Where(c => c.Status == ConsultationStatus.Pending)
            .OrderBy(c => c.CreatedAtUtc)
            .Select(c => new PendingConsultationDto(c.Id, c.CustomerId, c.CustomerNote, c.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
