using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Auth.Queries
{
    public record MeDto(int UserId, string Email, string FullName, string? Provider, bool IsActive);

    public record GetMeQuery(int UserId) : IRequest<MeDto>;
}
