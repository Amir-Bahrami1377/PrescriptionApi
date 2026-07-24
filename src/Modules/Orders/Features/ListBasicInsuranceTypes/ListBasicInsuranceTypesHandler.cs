using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListBasicInsuranceTypes;

public sealed class ListBasicInsuranceTypesHandler : IRequestHandler<ListBasicInsuranceTypesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(ListBasicInsuranceTypesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(Enum.GetNames<BasicInsuranceType>());
}
