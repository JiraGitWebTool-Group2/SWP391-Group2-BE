using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Auth.Command
{
    public record LogoutCommand(string RefreshToken) : IRequest<Unit>;
}
