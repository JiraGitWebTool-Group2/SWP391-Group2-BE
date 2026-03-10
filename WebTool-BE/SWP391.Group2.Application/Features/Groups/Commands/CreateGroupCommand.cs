using MediatR;
using SWP391.Group2.Application.Features.Groups.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Groups.Commands
{
    public record CreateGroupCommand(string GroupName, string? Description, int ClassId) : IRequest<GroupDto>;
}
