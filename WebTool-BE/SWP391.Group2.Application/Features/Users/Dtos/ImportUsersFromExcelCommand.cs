namespace SWP391.Group2.Application.Features.Users.Dtos
{
    public class ImportUsersExcelResultDto
    {
        public int TotalRows { get; set; }
        public int Inserted { get; set; }
        public int Skipped { get; set; }
        public List<ImportUsersExcelRowErrorDto> Errors { get; set; } = new();
    }

    public class ImportUsersExcelRowErrorDto
    {
        public int RowNumber { get; set; }
        public string? Username { get; set; }
        public string Error { get; set; } = default!;
    }
}