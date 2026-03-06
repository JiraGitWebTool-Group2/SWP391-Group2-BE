using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Reports.Dtos;

namespace SWP391.Group2.Application.Features.Reports.Queries
{
    public record GetReportByIdQuery(int Id) : IRequest<ReportDto?>;

    public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, ReportDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetReportByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
        {
            var report = await _context.Reports
                .AsNoTracking()
                .Where(x => x.ReportId == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken);

            return report;
        }
    }
}