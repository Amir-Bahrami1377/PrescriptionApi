using MediatR;

namespace Prescription.Modules.Catalog.Features.DeleteTest;

public sealed record DeleteTestCommand(Guid Id) : IRequest;
