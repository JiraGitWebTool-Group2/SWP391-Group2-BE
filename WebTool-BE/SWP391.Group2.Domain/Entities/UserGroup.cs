using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class UserGroup
    {
        public int UserGroupId { get; set; }

        public int UserId { get; set; }

        public int GroupId { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime JoinedAt { get; set; }

        public User User { get; set; } = default!;

        public Group Group { get; set; } = default!;

        public Role Role { get; set; } = default!;
    }
}
