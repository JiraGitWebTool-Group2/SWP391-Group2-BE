using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Column("class_id")]
        public int? ClassId { get; set; }   

        public DateTime CreatedAt { get; set; }

        // Navigation
        public Class? Class { get; set; }  

        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }

}
