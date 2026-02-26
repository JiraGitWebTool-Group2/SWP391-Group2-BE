using MediatR;
using SWP391.Group2.Application.Abstractions.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Auth
{
    public record GoogleLoginCommand(string IdToken) : IRequest<TokenPair>;
}
