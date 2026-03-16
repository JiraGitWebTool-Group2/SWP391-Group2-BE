namespace SWP391.Group2.Api.Contracts.Reports
{
    public record GenerateProgressReportResponse(
        int ReportId,
        int ProjectId,
        int CompletionRate,
        int DoneTasks,
        int InProgressTasks,
        int OverdueTasks,
        DateTime GeneratedAt
    );
}