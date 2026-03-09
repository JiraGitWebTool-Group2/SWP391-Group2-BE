namespace SWP391.Group2.Api.Contracts.Classes;

public class AssignStudentsBulkRequest
{
    public List<int> StudentIds { get; set; } = new();
}