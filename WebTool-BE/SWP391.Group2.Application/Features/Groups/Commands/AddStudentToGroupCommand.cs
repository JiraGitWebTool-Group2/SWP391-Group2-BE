using MediatR;
using SWP391.Group2.Application.Features.Groups.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Groups.Commands
{
    public class AddStudentToGroupCommand : IRequest<StudentDto>
    {
        public int GroupId { get; }
        public int UserId { get; }
        public int RoleId { get; }

        // Constructor to accept groupId, userId, and roleId
        public AddStudentToGroupCommand(int groupId, int userId, int roleId)
        {
            GroupId = groupId;
            UserId = userId;
            RoleId = roleId;
        }
    }
}
