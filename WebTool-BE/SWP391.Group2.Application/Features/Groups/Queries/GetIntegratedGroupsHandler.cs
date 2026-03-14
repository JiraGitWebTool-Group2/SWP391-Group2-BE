using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Groups.Dtos;

namespace SWP391.Group2.Application.Features.Groups.Queries;

public sealed class GetIntegratedGroupsHandler
    : IRequestHandler<GetIntegratedGroupsQuery, List<IntegratedGroupDto>>
{
    private readonly IApplicationDbContext _context;

    public GetIntegratedGroupsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IntegratedGroupDto>> Handle(
        GetIntegratedGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var rawData = await
            (from g in _context.Groups
             join p in _context.Projects on g.GroupId equals p.GroupId
             join pi in _context.ProjectIntegrations on p.ProjectId equals pi.ProjectId
             join c in _context.Classes on g.ClassId equals c.ClassId into classJoin
             from c in classJoin.DefaultIfEmpty()
             select new
             {
                 g.GroupId,
                 g.GroupName,
                 g.ClassId,
                 ClassCode = c != null ? c.ClassCode : null,
                 p.ProjectId,
                 p.ProjectName,
                 pi.Provider
             })
            .ToListAsync(cancellationToken);

        var result = rawData
            .GroupBy(x => new
            {
                x.GroupId,
                x.GroupName,
                x.ClassId,
                x.ClassCode,
                x.ProjectId,
                x.ProjectName
            })
            .Select(x => new IntegratedGroupDto
            {
                GroupId = x.Key.GroupId,
                GroupName = x.Key.GroupName,
                ClassId = x.Key.ClassId,
                ClassCode = x.Key.ClassCode,
                ProjectId = x.Key.ProjectId,
                ProjectName = x.Key.ProjectName,
                Integrations = x
                    .Select(i => i.Provider)
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Distinct()
                    .ToList()
            })
            .OrderBy(x => x.GroupId)
            .ToList();

        return result;
    }
}