using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Lecturers.Queries;

public class GetLecturerClassesQueryHandler
    : IRequestHandler<GetLecturerClassesQuery, List<LecturerClassDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLecturerClassesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    

    public async Task<List<LecturerClassDto>> Handle(
        GetLecturerClassesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Classes
            .Where(c => c.LecturerUserId == request.LecturerId)
            .Select(c => new LecturerClassDto
            {
                ClassId = c.ClassId,
                ClassCode = c.ClassCode
            })
            .ToListAsync(cancellationToken);
    }
}