using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Reports.Dtos;
using SWP391.Group2.Domain.Entities;
using System.Text.Json;

namespace SWP391.Group2.Application.Features.Reports.Commands
{
    public class GenerateProgressReportHandler : IRequestHandler<GenerateProgressReportCommand, GeneratedProgressReportDto>
    {
        private readonly IApplicationDbContext _db;

        public GenerateProgressReportHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GeneratedProgressReportDto> Handle(GenerateProgressReportCommand request, CancellationToken ct)
        {
            if (request.ProjectId <= 0)
                throw new ArgumentException("projectId must be greater than 0.");

            if (request.StartDate > request.EndDate)
                throw new ArgumentException("startDate must be less than or equal to endDate.");

            var viewType = (request.ViewType ?? string.Empty).Trim().ToUpperInvariant();
            if (viewType is not ("GROUP" or "INDIVIDUAL"))
                throw new ArgumentException("viewType must be GROUP or INDIVIDUAL.");

            var project = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId, ct);

            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var start = request.StartDate.ToDateTime(TimeOnly.MinValue);
            var endExclusive = request.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var issuesQuery = _db.JiraIssues
                .AsNoTracking()
                .Where(x => x.ProjectId == request.ProjectId)
                .Where(x => x.CreatedAt >= start && x.CreatedAt < endExclusive);

            if (request.Members is { Count: > 0 })
            {
                issuesQuery = issuesQuery.Where(x =>
                    x.AssigneeUserId.HasValue &&
                    request.Members.Contains(x.AssigneeUserId.Value));
            }

            var issues = await issuesQuery.ToListAsync(ct);

            var doneTasks = issues.Count(x => x.Status == "DONE");

            var inProgressTasks = issues.Count(x =>
                x.Status == "IN_PROGRESS" ||
                x.Status == "IN_REVIEW" ||
                x.Status == "BLOCKED");

            var overdueTasks = issues.Count(x =>
                x.Status != "DONE" &&
                x.DueDate.HasValue &&
                x.DueDate.Value < request.EndDate);

            var totalTasks = issues.Count;
            var completionRate = totalTasks == 0
                ? 0
                : (int)Math.Round(doneTasks * 100.0 / totalTasks, MidpointRounding.AwayFromZero);

            var generatedAt = DateTime.UtcNow;

            var contentPayload = new
            {
                source = "SYNCED_DB",
                filters = new
                {
                    request.ProjectId,
                    request.StartDate,
                    request.EndDate,
                    request.Members,
                    ViewType = viewType
                },
                summary = new
                {
                    completionRate,
                    doneTasks,
                    inProgressTasks,
                    overdueTasks
                }
            };

            var report = new Report
            {
                ProjectId = request.ProjectId,
                CreatedByUserId = request.CreatedByUserId,
                SnapshotId = null,
                Title = $"Progress Report Preview ({request.StartDate:yyyy-MM-dd} - {request.EndDate:yyyy-MM-dd})",
                Content = JsonSerializer.Serialize(contentPayload),
                Status = "DRAFT",
                CreatedAt = generatedAt,
                UpdatedAt = generatedAt
            };

            _db.Reports.Add(report);
            await _db.SaveChangesAsync(ct);

            return new GeneratedProgressReportDto(
                report.ReportId,
                request.ProjectId,
                completionRate,
                doneTasks,
                inProgressTasks,
                overdueTasks,
                generatedAt
            );
        }
    }
}