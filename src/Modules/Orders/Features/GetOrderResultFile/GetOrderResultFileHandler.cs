using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.GetOrderResultFile;

public sealed class GetOrderResultFileHandler(OrdersDbContext dbContext, IFileStorageService fileStorageService)
    : IRequestHandler<GetOrderResultFileQuery, GetOrderResultFileResponse>
{
    private static readonly TimeSpan UrlExpiry = TimeSpan.FromMinutes(15);

    public async Task<GetOrderResultFileResponse> Handle(GetOrderResultFileQuery request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var isOwner = order.CustomerId == request.RequestingUserId;
        var isStaff = request.RequestingUserRole is "Doctor" or "Admin";

        if (!isOwner && !isStaff)
        {
            throw new UnauthorizedDomainException("این سفارش متعلق به شما نیست.");
        }

        if (order.ResultFileKey is not { } resultFileKey)
        {
            throw new ConflictException("هنوز جواب آزمایشی برای این سفارش بارگذاری نشده است.");
        }

        var url = await fileStorageService.GetPresignedUrlAsync(StorageBuckets.TestResults, resultFileKey, UrlExpiry, cancellationToken);

        return new GetOrderResultFileResponse(url, (int)UrlExpiry.TotalSeconds);
    }
}
