using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Infrastructure.Persistence;
using SWP391.Group2.Application.Abstractions.Auth;
using SWP391.Group2.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddScoped<ITokenService, JwtTokenService>();

            return services;
        }
    }
}
