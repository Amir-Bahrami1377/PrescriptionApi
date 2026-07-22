using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.UploadTestResult;

public sealed class UploadTestResultHandler(OrdersDbContext dbContext, IFileStorageService fileStorageService)
    : IRequestHandler<UploadTestResultCommand>
{
    public async Task Handle(UploadTestResultCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var objectKey = $"{order.Id}/result-{Guid.NewGuid()}-{request.FileName}";
        await fileStorageService.UploadAsync(
            StorageBuckets.TestResults,
            objectKey,
            request.FileContent,
            request.ContentType,
            cancellationToken);

        order.UploadResult(objectKey);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
