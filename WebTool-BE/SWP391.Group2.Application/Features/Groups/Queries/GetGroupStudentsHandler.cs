using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Groups.Dtos;

namespace SWP391.Group2.Application.Features.Groups.Queries;

public class GetGroupStudentsHandler : IRequestHandler<GetGroupStudentsQuery, List<GroupStudentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGroupStudentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GroupStudentDto>> Handle(GetGroupStudentsQuery request, CancellationToken cancellationToken)
    {
        var groupExists = await _context.Groups
            .AnyAsync(g => g.GroupId == request.GroupId, cancellationToken);

        if (!groupExists)
        {
            throw new KeyNotFoundException($"Group with id {request.GroupId} was not found.");
        }

        var students = await (
            from ug in _context.UserGroups
            join u in _context.Users on ug.UserId equals u.UserId
            join r in _context.Roles on ug.RoleId equals r.RoleId
            where ug.GroupId == request.GroupId
                  && ug.IsActive
                  && u.SystemRole == "STUDENT"
            orderby u.FullName
            select new GroupStudentDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                SystemRole = u.SystemRole,
                IsActive = ug.IsActive,
                JoinedAt = ug.JoinedAt,
                GroupRole = r.RoleName
            }
        ).ToListAsync(cancellationToken);

        return students;
    }
}