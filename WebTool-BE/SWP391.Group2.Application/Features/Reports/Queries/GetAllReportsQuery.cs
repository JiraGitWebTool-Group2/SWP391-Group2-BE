using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Reports.Dtos;

namespace SWP391.Group2.Application.Features.Reports.Queries
{
    public record GetAllReportsQuery : IRequest<List<ReportDto>>;

    public class GetAllReportsQueryHandler : IRequestHandler<GetAllReportsQuery, List<ReportDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllReportsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReportDto>> Handle(GetAllReportsQuery request, CancellationToken cancellationToken)
        {
            var reports = await _context.Reports
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReportDto
                {
                    ReportId = x.ReportId,
                    ProjectId = x.ProjectId,
                    CreatedByUserId = x.CreatedByUserId,
                    SnapshotId = x.SnapshotId,
                    Title = x.Title,
                    Content = x.Content,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return reports;
        }
    }
}