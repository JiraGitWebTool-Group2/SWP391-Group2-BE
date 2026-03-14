using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Users.Dtos;

namespace SWP391.Group2.Application.Features.Users.Queries
{
    public sealed class GetMyGroupHandler : IRequestHandler<GetMyGroupQuery, MyGroupDto>
    {
        private readonly IApplicationDbContext _db;

        public GetMyGroupHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<MyGroupDto> Handle(GetMyGroupQuery request, CancellationToken cancellationToken)
        {
            var result = await
                (from ug in _db.UserGroups.AsNoTracking()
                 join g in _db.Groups.AsNoTracking() on ug.GroupId equals g.GroupId
                 where ug.UserId == request.UserId && ug.IsActive
                 orderby ug.JoinedAt descending
                 select new MyGroupDto
                 {
                     GroupId = g.GroupId,
                     GroupName = g.GroupName,
                     RoleId = ug.RoleId
                 })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                throw new KeyNotFoundException("User is not assigned to any group.");

            return result;
        }
    }
}