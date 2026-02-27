using MediatR;
using SWP391.Group2.Application.Features.Repositories.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Repositories.Command
{
    public record CreateRepositoryCommand(
        int ProjectId,
        string RepoName,
        string? RepoUrl,
        string? DefaultBranch
    ) : IRequest<RepositoryDto>;
}
