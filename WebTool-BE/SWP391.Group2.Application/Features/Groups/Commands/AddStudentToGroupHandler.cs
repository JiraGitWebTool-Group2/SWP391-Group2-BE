using MediatR;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Groups.Dtos;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Groups.Commands
{
    public class AddStudentToGroupHandler : IRequestHandler<AddStudentToGroupCommand, StudentDto>
    {
        private readonly IApplicationDbContext _context;

        public AddStudentToGroupHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentDto> Handle(AddStudentToGroupCommand request, CancellationToken cancellationToken)
        {
            // Fetch the group information
            var group = await _context.Groups.FindAsync(request.GroupId);
            if (group == null)
            {
                return null;  // If the group is not found
            }

            // Fetch the user (student) information
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return null;  // If the user (student) is not found
            }

            // Fetch the role information
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
            {
                return null;  // If the role is not found
            }

            // Add the user to the group with the specified role
            var userGroup = new UserGroup
            {
                UserId = request.UserId,
                GroupId = request.GroupId,
                RoleId = request.RoleId,
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                //CreatedAt = DateTime.UtcNow
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync(cancellationToken);

            // Return the DTO with the necessary information
            return new StudentDto
            {
                StudentId = user.UserId,  // userId from Users table
                StudentName = user.FullName,  // fullName from Users table
                StudentEmail = user.Email,  // email from Users table
                GroupId = group.GroupId,  // groupId from Groups table
                GroupName = group.GroupName,  // groupName from Groups table
                JoinedAt = userGroup.JoinedAt  // joinedAt from UserGroups table
            };
        }
    }

}
