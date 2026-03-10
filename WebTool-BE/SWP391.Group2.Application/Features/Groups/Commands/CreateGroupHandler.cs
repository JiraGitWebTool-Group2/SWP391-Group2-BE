using MediatR;
using Microsoft.EntityFrameworkCore;
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

            // check class tồn tại
            var classExists = await _db.Classes
                .AnyAsync(c => c.ClassId == request.ClassId, cancellationToken);

            if (!classExists)
                throw new InvalidOperationException("Class not found.");

            // check duplicate group name
            var exists = await _db.Groups
                .AnyAsync(g => g.GroupName == name, cancellationToken);

            if (exists)
                throw new InvalidOperationException("Group name already exists.");

            var entity = new Group
            {
                GroupName = name,
                Description = request.Description,
                ClassId = request.ClassId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Groups.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

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
