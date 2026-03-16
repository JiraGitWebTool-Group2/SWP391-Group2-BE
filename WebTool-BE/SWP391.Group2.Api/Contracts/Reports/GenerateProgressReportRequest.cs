namespace SWP391.Group2.Api.Contracts.Reports
{
    public record GenerateProgressReportRequest(
        int ProjectId,
        DateOnly StartDate,
        DateOnly EndDate,
        List<int> Members,
        string ViewType
    );
}