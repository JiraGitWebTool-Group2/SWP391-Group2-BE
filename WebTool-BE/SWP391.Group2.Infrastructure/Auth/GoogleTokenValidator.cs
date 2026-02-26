using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using SWP391.Group2.Application.Abstractions.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Infrastructure.Auth
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly IConfiguration _config;

        public GoogleTokenValidator(IConfiguration config)
        {
            _config = config;
        }

        public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken ct = default)
        {
            var clientId = _config["GoogleAuth:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("GoogleAuth:ClientId is not configured.");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid Google token.");
            }

            var email = payload.Email ?? "";
            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedAccessException("Google token has no email.");

            return new GoogleUserInfo(email, payload.Name, payload.Subject);
        }
    }
}
