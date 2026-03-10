//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using SWP391.Group2.Application.Abstractions;
//using SWP391.Group2.Application.Features.Integrations.Dtos;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SWP391.Group2.Application.Features.Integrations.Commands
//{
//    public class UpdateProjectIntegrationHandler : IRequestHandler<UpdateProjectIntegrationCommand, ProjectIntegrationDto?>
//    {
//        private readonly IApplicationDbContext _db;

//        public UpdateProjectIntegrationHandler(IApplicationDbContext db)
//        {
//            _db = db;
//        }

//        public async Task<ProjectIntegrationDto?> Handle(UpdateProjectIntegrationCommand request, CancellationToken cancellationToken)
//        {
//            // Optional: check group tồn tại (để trả 404 group)
//            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == request.GroupId, cancellationToken);
//            if (!groupExists) return null;

//            var project = await _db.Projects
//                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId && p.GroupId == request.GroupId, cancellationToken);

//            if (project is null) return null;

//            project.JiraProjectKey = request.JiraProjectKey;
//            project.GithubOrg = request.GithubOrg;

//            await _db.SaveChangesAsync(cancellationToken);

//            return new ProjectIntegrationDto(
//                project.ProjectId,
//                project.GroupId,
//                project.ProjectName,
//                project.JiraProjectKey,
//                project.GithubOrg
//            );
//        }
//    }
//}
