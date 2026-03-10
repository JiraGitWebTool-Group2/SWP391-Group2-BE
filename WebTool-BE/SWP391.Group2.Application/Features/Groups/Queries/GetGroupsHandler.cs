using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Groups.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Groups.Queries
{
    public class GetGroupsHandler : IRequestHandler<GetGroupsQuery, List<GroupDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetGroupsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<GroupDto>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
        {
            return await _db.Groups.AsNoTracking()
            .OrderByDescending(g => g.GroupId)
            .Select(g => new GroupDto(
            g.GroupId,
            g.GroupName,
            g.Description,
            g.ClassId,
            g.CreatedAt
            ))
            .ToListAsync(cancellationToken);
        }
    }
}
