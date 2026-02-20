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

            // DB có UNIQUE(group_name) nên check trước cho đỡ nổ SQL exception
            var exists = await _db.Groups.AnyAsync(g => g.GroupName == name, cancellationToken);
            if (exists)
                throw new InvalidOperationException("Group name already exists.");

            var entity = new Group
            {
                GroupName = name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
                // CreatedAt: DB default SYSDATETIME() đã set, nhưng entity vẫn cần value để DTO trả về.
                // Nếu bạn muốn chính xác theo DB, có thể query lại sau SaveChanges.
            };

            _db.Groups.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return new GroupDto(entity.GroupId, entity.GroupName, entity.Description, entity.CreatedAt);
        }
    }
}
