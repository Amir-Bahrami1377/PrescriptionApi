using MediatR;

namespace Prescription.Modules.Orders.Features.UploadConsultationTestResult;

public sealed record UploadConsultationTestResultCommand(
    Guid OrderId,
    Guid CustomerId,
    Stream FileContent,
    string FileName,
    string ContentType) : IRequest;
