namespace SWP391.Group2.Api.Contracts.Semesters;

public class UpdateSemesterRequest
{
    public string Code { get; set; } = null!;
    //public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = null!;
}