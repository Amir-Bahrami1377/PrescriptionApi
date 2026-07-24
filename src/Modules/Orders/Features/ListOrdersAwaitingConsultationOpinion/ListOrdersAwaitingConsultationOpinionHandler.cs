using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListOrdersAwaitingConsultationOpinion;

public sealed class ListOrdersAwaitingConsultationOpinionHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListOrdersAwaitingConsultationOpinionQuery, IReadOnlyList<AwaitingConsultationOpinionOrderDto>>
{
    public async Task<IReadOnlyList<AwaitingConsultationOpinionOrderDto>> Handle(ListOrdersAwaitingConsultationOpinionQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.AwaitingConsultationOpinion)
            .OrderBy(o => o.UpdatedAtUtc)
            .Select(o => new AwaitingConsultationOpinionOrderDto(
                o.Id,
                o.CustomerId,
                o.LabTestIds,
                o.CustomerNote,
                o.BasicInsurance,
                o.DoctorId,
                o.CreatedAtUtc,
                o.UpdatedAtUtc!.Value))
            .ToListAsync(cancellationToken);
    }
}
