namespace SWP391.Group2.Application.Features.Reports.Dtos
{
    public record GeneratedProgressReportDto(
        int ReportId,
        int ProjectId,
        int CompletionRate,
        int DoneTasks,
        int InProgressTasks,
        int OverdueTasks,
        DateTime GeneratedAt
    );
}