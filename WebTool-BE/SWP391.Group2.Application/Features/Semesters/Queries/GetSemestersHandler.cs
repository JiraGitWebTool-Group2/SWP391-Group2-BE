using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Semesters.Dtos;

namespace SWP391.Group2.Application.Features.Semesters.Queries;

public class GetSemestersHandler : IRequestHandler<GetSemestersQuery, List<SemesterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSemestersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SemesterDto>> Handle(GetSemestersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Semesters
            .AsNoTracking()
            .OrderByDescending(x => x.SemesterId)
            .Select(x => new SemesterDto
            {
                SemesterId = x.SemesterId,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}