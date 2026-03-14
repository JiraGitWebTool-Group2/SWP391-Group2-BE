namespace SWP391.Group2.Application.Features.Users.Dtos
{
    public sealed class MyGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}