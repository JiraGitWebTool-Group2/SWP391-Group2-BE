using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Groups.Dtos
{
    public record GroupDto(
        int GroupId,
        string GroupName,
        string? Description,
        DateTime CreatedAt
    );
}
