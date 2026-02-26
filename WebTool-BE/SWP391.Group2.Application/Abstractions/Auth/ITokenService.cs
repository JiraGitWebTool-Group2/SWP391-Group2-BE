using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions.Auth
{
    public record TokenPair(string AccessToken, string RefreshToken);

    public interface ITokenService
    {
        TokenPair CreateTokenPair(User user);
        string HashRefreshToken(string refreshToken);
    }
}
