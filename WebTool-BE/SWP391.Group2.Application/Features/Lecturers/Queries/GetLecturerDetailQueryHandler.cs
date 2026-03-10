using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Lecturers.Queries;

public class GetLecturerDetailQueryHandler
    : IRequestHandler<GetLecturerDetailQuery, LecturerDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLecturerDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LecturerDetailDto?> Handle(
        GetLecturerDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .Where(x => x.UserId == request.LecturerId)
            .Select(x => new LecturerDetailDto
            {
                LecturerId = x.UserId,
                Email = x.Email,
                FullName = x.FullName
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}