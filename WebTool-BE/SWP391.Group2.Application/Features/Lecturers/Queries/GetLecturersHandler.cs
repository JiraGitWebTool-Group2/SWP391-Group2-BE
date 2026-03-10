using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Lecturers.Dtos;

namespace SWP391.Group2.Application.Features.Lecturers.Queries;

public class GetLecturersHandler : IRequestHandler<GetLecturersQuery, List<LecturerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLecturersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LecturerDto>> Handle(GetLecturersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.System_Role == "LECTURER")
            .OrderBy(x => x.FullName)
            .Select(x => new LecturerDto
            {
                LecturerId = x.UserId,
                Email = x.Email,
                FullName = x.FullName,
                SystemRole = x.System_Role,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}