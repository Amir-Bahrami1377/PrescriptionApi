using MediatR;

namespace Prescription.Modules.Orders.Features.ListBasicInsuranceTypes;

/// <summary>Lets the frontend populate the basic-insurance dropdown from the real enum instead of guessing member names.</summary>
public sealed record ListBasicInsuranceTypesQuery : IRequest<IReadOnlyList<string>>;
