using MediatR;

namespace Prescription.Modules.Catalog.Features.CreateTest;

public sealed record CreateTestCommand(string Name, string? Description, long PriceInRials) : IRequest<CreateTestResponse>;

public sealed record CreateTestResponse(Guid Id);
