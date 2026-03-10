using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = default!;

        public ICollection<UserGroup> UserGroups { get; set; }
            = new List<UserGroup>();
    }
}
