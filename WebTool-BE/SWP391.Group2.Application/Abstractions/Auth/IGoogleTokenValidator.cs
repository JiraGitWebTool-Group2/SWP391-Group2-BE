using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions.Auth
{
    public record GoogleUserInfo(string Email, string? FullName, string Subject);

    public interface IGoogleTokenValidator
    {
        Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken ct = default);
    }
}
