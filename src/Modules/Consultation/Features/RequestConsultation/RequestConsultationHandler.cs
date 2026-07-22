using MediatR;
using Prescription.Modules.Consultation.Domain;
using Prescription.Modules.Consultation.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Consultation.Features.RequestConsultation;

public sealed class RequestConsultationHandler(ConsultationDbContext dbContext, IFileStorageService fileStorageService)
    : IRequestHandler<RequestConsultationCommand, RequestConsultationResponse>
{
    public async Task<RequestConsultationResponse> Handle(RequestConsultationCommand request, CancellationToken cancellationToken)
    {
        var objectKey = $"{Guid.NewGuid()}-{request.FileName}";
        await fileStorageService.UploadAsync(
            StorageBuckets.ConsultationImages,
            objectKey,
            request.FileContent,
            request.ContentType,
            cancellationToken);

        var consultation = ConsultationRequest.Create(request.CustomerId, objectKey, request.Note);

        dbContext.ConsultationRequests.Add(consultation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RequestConsultationResponse(consultation.Id);
    }
}
