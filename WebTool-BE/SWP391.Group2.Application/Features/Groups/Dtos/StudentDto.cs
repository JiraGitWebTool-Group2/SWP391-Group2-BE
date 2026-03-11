using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Groups.Dtos
{
    public class StudentDto
    {
        public int StudentId { get; set; }  // userId from Users table
        public string StudentName { get; set; }  // fullName from Users table
        public string StudentEmail { get; set; }  // email from Users table
        public int GroupId { get; set; }  // groupId from Groups table
        public string GroupName { get; set; }  // groupName from Groups table
        public DateTime JoinedAt { get; set; }  // from UserGroups table
    }
}
