using MediatR;

namespace Prescription.Modules.Catalog.Features.UpdateTest;

public sealed record UpdateTestCommand(Guid Id, string Name, string? Description, long PriceInRials, bool IsActive) : IRequest;
