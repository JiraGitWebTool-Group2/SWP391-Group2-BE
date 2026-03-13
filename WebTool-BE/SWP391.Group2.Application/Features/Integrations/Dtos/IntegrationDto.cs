using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Integrations.Dtos
{
    public record IntegrationDto(
        int IntegrationId,
        int ProjectId,
        string Provider,
        string? BaseUrl,
        string? ProjectKey,
        string? Org,
        bool HasToken,
        int? CreatedByUserId,
        string? LinkedAccount,
        string? VisibilityStatus,
        DateTime? LastVerifiedAt,
        string? VerificationNote,

        string? JiraStoryPointsFieldKey,
        string? JiraSprintFieldKey,
        string? JiraBoardId,
        DateTime? LastJiraSyncAt,

        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
