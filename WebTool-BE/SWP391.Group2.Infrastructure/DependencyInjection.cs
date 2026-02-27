using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Auth;
using SWP391.Group2.Application.Abstractions.Jobs;
using SWP391.Group2.Infrastructure.Auth;
using SWP391.Group2.Infrastructure.Jobs;
using SWP391.Group2.Infrastructure.Persistence;

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

            services.AddSingleton<BackgroundJobQueue>();
            services.AddSingleton<IBackgroundJobQueue>(sp => sp.GetRequiredService<BackgroundJobQueue>());
            services.AddHostedService<SyncRunWorker>();

            return services;
        }
    }
}
