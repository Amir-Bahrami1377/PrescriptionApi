using MediatR;

namespace Prescription.Modules.Catalog.Features.CreateTest;

public sealed record CreateTestCommand(string Name, string? Description) : IRequest<CreateTestResponse>;

public sealed record CreateTestResponse(Guid Id);
