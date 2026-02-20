using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Integrations.Dtos
{
    public record ProjectIntegrationDto(
        int ProjectId,
        int GroupId,
        string ProjectName,
        string? JiraProjectKey,
        string? GithubOrg
    );
}
