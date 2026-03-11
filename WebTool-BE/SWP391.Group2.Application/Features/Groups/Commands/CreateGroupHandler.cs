using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Groups.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Groups.Commands
{
    public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, GroupDto>
    {
        private readonly IApplicationDbContext _db;

        public CreateGroupHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GroupDto> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
        {
            var name = (request.GroupName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("GroupName is required.");

            // Lấy class + lecturer của class
            var classEntity = await _db.Classes
                .FirstOrDefaultAsync(c => c.ClassId == request.ClassId, cancellationToken);

            if (classEntity == null)
                throw new InvalidOperationException("Class not found.");

            // Check duplicate group name
            var exists = await _db.Groups
                .AnyAsync(g => g.GroupName == name, cancellationToken);

            if (exists)
                throw new InvalidOperationException("Group name already exists.");

            // Tạo group
            var entity = new Group
            {
                GroupName = name,
                Description = request.Description,
                ClassId = request.ClassId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Groups.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            // Nếu class đã có lecturer thì tự add lecturer vào group
            if (classEntity.LecturerUserId.HasValue)
            {
                var lecturerId = classEntity.LecturerUserId.Value;

                var lecturer = await _db.Users
                    .FirstOrDefaultAsync(
                        u => u.UserId == lecturerId &&
                             u.SystemRole == "LECTURER" &&
                             u.IsActive,
                        cancellationToken);

                {
                    // Set role_id = 2 (assuming role_id for 'LECTURER' is 2)
                    var lecturerRoleId = 2;

                    var existedUserGroup = await _db.UserGroups
                        .AnyAsync(
                            ug => ug.UserId == lecturerId && ug.GroupId == entity.GroupId,
                            cancellationToken);

                    if (!existedUserGroup)
                    {
                        var lecturerInGroup = new UserGroup
                        {
                            UserId = lecturerId,
                            GroupId = entity.GroupId,
                            RoleId = lecturerRoleId,  // Set role_id to 2 for 'LECTURER'
                            IsActive = true,
                            JoinedAt = DateTime.UtcNow
                        };

                        _db.UserGroups.Add(lecturerInGroup);
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            return new GroupDto(
                entity.GroupId,
                entity.GroupName,
                entity.Description,
                entity.ClassId,
                entity.CreatedAt
            );
        }
    }
}