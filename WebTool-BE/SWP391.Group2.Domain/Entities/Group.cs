using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
