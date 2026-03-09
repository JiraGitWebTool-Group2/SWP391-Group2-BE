using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Semesters.Dtos;

namespace SWP391.Group2.Application.Features.Semesters.Queries;

public class GetSemesterByIdHandler : IRequestHandler<GetSemesterByIdQuery, SemesterDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSemesterByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SemesterDto?> Handle(GetSemesterByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Semesters
            .AsNoTracking()
            .Where(x => x.SemesterId == request.SemesterId)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}