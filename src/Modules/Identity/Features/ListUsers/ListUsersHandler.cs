using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Identity.Infrastructure.Persistence;

namespace Prescription.Modules.Identity.Features.ListUsers;

public sealed class ListUsersHandler(IdentityDbContext dbContext) : IRequestHandler<ListUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    public async Task<IReadOnlyList<UserSummaryDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsNoTracking().Where(u => u.IsActive);

        if (request.Role is { } role)
        {
            query = query.Where(u => u.Role == role);
        }

        return await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new UserSummaryDto(
                u.Id,
                u.PhoneNumber,
                u.Role.ToString(),
                u.FullName,
                u.IsProfileCompleted,
                u.DoctorFeeInRials,
                u.IsSpecialPatient,
                u.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
