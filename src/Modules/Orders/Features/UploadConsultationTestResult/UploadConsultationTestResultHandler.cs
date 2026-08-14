using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Notifications;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.UploadConsultationTestResult;

public sealed class UploadConsultationTestResultHandler(
    OrdersDbContext dbContext,
    IFileStorageService fileStorageService,
    IOrderStatusNotifier orderStatusNotifier)
    : IRequestHandler<UploadConsultationTestResultCommand>
{
    public async Task Handle(UploadConsultationTestResultCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.CustomerId != request.CustomerId)
        {
            throw new UnauthorizedDomainException("این سفارش متعلق به شما نیست.");
        }

        var objectKey = $"{order.Id}/consultation-result-{Guid.NewGuid()}-{request.FileName}";
        await fileStorageService.UploadAsync(
            StorageBuckets.TestResults,
            objectKey,
            request.FileContent,
            request.ContentType,
            cancellationToken);

        order.UploadConsultationTestResult(objectKey);
        await dbContext.SaveChangesAsync(cancellationToken);

        await orderStatusNotifier.NotifyCustomerAsync(order, cancellationToken);
    }
}
