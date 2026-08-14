using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Notifications;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.DoctorReviewOrder;

public sealed class DoctorReviewOrderHandler(
    OrdersDbContext dbContext,
    IIdentityLookup identityLookup,
    IOrderStatusNotifier orderStatusNotifier)
    : IRequestHandler<DoctorReviewOrderCommand>
{
    public async Task Handle(DoctorReviewOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (request.Approve)
        {
            var doctorFee = await identityLookup.GetDoctorFeeAsync(request.DoctorId, cancellationToken);
            if (doctorFee?.FeeInRials is not { } feeInRials)
            {
                throw new DomainException("پزشک هنوز هزینه ویزیت خود را در پنل مدیریتی تعیین نکرده است.");
            }

            if (order.RequestsConsultation && doctorFee.ConsultationFeeInRials is null)
            {
                throw new DomainException("پزشک هنوز هزینه مشاوره خود را در پنل مدیریتی تعیین نکرده است.");
            }

            order.Approve(request.DoctorId, feeInRials, doctorFee.ConsultationFeeInRials);
        }
        else
        {
            order.Reject(request.DoctorId, request.RejectionReason!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await orderStatusNotifier.NotifyCustomerAsync(order, cancellationToken);
    }
}
