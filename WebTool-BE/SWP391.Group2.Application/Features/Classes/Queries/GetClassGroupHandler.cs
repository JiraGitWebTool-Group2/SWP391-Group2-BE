using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;
using SWP391.Group2.Application.Features.Groups.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Classes.Queries
{
    public class GetClassGroupHandler
    : IRequestHandler<GetClassGroupQuery, List<ClassGroupProjectDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetClassGroupHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ClassGroupProjectDto>> Handle(
            GetClassGroupQuery request,
            CancellationToken cancellationToken)
        {
            return await _db.Groups
                .AsNoTracking()
                .Where(g => g.ClassId == request.ClassId)
                .Select(g => new ClassGroupProjectDto(
                    g.GroupId,
                    g.GroupName,
                    g.Projects
                        .Select(p => (int?)p.ProjectId)
                        .FirstOrDefault(),
                    g.Projects
                        .Select(p => p.ProjectName)
                        .FirstOrDefault()
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
