using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders.Features.ListOrdersByCustomer;

public sealed class ListOrdersByCustomerHandler(OrdersDbContext dbContext)
    : IRequestHandler<ListOrdersByCustomerQuery, IReadOnlyList<CustomerOrderDto>>
{
    public async Task<IReadOnlyList<CustomerOrderDto>> Handle(ListOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new CustomerOrderDto(
                o.Id,
                o.LabTestIds,
                o.BasicInsurance,
                o.IsForThirdParty,
                o.RequestsConsultation,
                o.Status.ToString(),
                o.PriceInRials,
                o.RejectionReason,
                o.PrescriptionReferenceNumber,
                o.ResultFileKey != null,
                o.ConsultationOpinion,
                o.CreatedAtUtc,
                o.CompletedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
