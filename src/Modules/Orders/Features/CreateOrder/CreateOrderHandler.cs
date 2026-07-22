using MediatR;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderHandler(
    OrdersDbContext dbContext,
    ICatalogLookup catalogLookup,
    IFileStorageService fileStorageService)
    : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var test = await catalogLookup.GetActiveTestAsync(request.LabTestId, cancellationToken)
            ?? throw new NotFoundException("LabTest", request.LabTestId);

        var objectKey = $"{Guid.NewGuid()}-{request.FileName}";
        await fileStorageService.UploadAsync(
            StorageBuckets.TestResults,
            objectKey,
            request.FileContent,
            request.ContentType,
            cancellationToken);

        var order = Order.Create(request.CustomerId, test.Id, test.PriceInRials, request.Note, objectKey);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(order.Id, order.Status.ToString());
    }
}
