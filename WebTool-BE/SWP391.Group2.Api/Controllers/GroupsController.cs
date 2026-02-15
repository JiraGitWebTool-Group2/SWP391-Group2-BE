using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Infrastructure.Persistence;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public GroupsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET api/groups
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _db.Groups.AsNoTracking().ToListAsync();
            return Ok(groups);
        }

        public record CreateGroupRequest(string GroupName, string? Description);

        // POST api/groups
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
                return BadRequest("GroupName is required.");

            // DB đã có unique group_name, nên cứ thử insert, trùng thì nó nổ.
            var group = new Group
            {
                GroupName = request.GroupName.Trim(),
                Description = request.Description
                // CreatedAt: nếu bạn cấu hình default DB thì không cần set
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            return Ok(group);
        }
    }
}
