using MediatR;

namespace Prescription.Modules.Catalog.Features.ListTests;

public sealed record ListTestsQuery(string? Search) : IRequest<IReadOnlyList<LabTestDto>>;

public sealed record LabTestDto(Guid Id, string Name, string? Description, long PriceInRials);
