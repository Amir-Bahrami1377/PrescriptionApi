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
        var requestedTestIds = request.LabTestIds.Distinct().ToList();

        var tests = await catalogLookup.GetActiveTestsAsync(requestedTestIds, cancellationToken);
        var foundIds = tests.Select(t => t.Id).ToHashSet();
        var missingIds = requestedTestIds.Where(id => !foundIds.Contains(id)).ToList();
        if (missingIds.Count > 0)
        {
            throw new NotFoundException("LabTest", string.Join(", ", missingIds));
        }

        string? objectKey = null;
        if (request.FileContent is not null)
        {
            objectKey = $"{Guid.NewGuid()}-{request.FileName}";
            await fileStorageService.UploadAsync(
                StorageBuckets.TestResults,
                objectKey,
                request.FileContent,
                request.ContentType!,
                cancellationToken);
        }

        var thirdParty = request.IsForThirdParty
            ? new ThirdPartyBeneficiary(request.ThirdPartyNationalCode!, request.ThirdPartyPhoneNumber!)
            : null;

        var order = Order.Create(
            request.CustomerId,
            requestedTestIds,
            request.Note,
            objectKey,
            request.BasicInsurance,
            request.SupplementaryInsurance,
            thirdParty);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(order.Id, order.Status.ToString());
    }
}
