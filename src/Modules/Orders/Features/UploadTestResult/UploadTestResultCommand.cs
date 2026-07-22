using MediatR;

namespace Prescription.Modules.Orders.Features.UploadTestResult;

public sealed record UploadTestResultCommand(
    Guid OrderId,
    Stream FileContent,
    string FileName,
    string ContentType) : IRequest;
