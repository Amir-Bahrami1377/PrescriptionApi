using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.GetOrderStatus;

public sealed class GetOrderStatusHandler(OrdersDbContext dbContext) : IRequestHandler<GetOrderStatusQuery, OrderStatusDto>
{
    public async Task<OrderStatusDto> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var isOwner = order.CustomerId == request.RequestingUserId;
        var isStaff = request.RequestingUserRole is "Doctor" or "Admin";

        if (!isOwner && !isStaff)
        {
            throw new UnauthorizedDomainException("این سفارش متعلق به شما نیست.");
        }

        return new OrderStatusDto(
            order.Id,
            order.LabTestIds,
            order.PriceInRials,
            order.Status.ToString(),
            order.RejectionReason,
            order.PrescriptionReferenceNumber,
            order.PaymentReferenceId,
            order.ResultFileKey is not null,
            order.BasicInsurance,
            order.SupplementaryInsurance,
            order.IsForThirdParty,
            order.ThirdPartyNationalCode,
            order.ThirdPartyPhoneNumber,
            order.CreatedAtUtc,
            order.CompletedAtUtc);
    }
}
