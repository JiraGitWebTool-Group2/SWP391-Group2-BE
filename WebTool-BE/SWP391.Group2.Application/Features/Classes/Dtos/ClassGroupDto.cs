using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Classes.Dtos
{
    public record ClassGroupProjectDto(
        int GroupId,
        string GroupName,
        int? ProjectId,
        string? ProjectName
    );
}
