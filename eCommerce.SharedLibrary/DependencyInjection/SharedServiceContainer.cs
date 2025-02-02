
using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Runtime.CompilerServices;

namespace eCommerce.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>
            (this IServiceCollection services, IConfiguration config, string filename) where TContext : DbContext
        {
            //Add generic database context
            services.AddDbContext<TContext>(options =>
            {
                options.UseSqlServer(config
                    .GetConnectionString("eCommerceConnection"), 
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());

            });
            //configure Serilog Logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{filename}-.text",
                restrictedToMinimumLevel:Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]{message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //Add JWT authentication
            JWTAuthenticationScheme.AddJWTAuthentication(services, config);

            return services;
        }
        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            //Add global exception handler
            app.UseMiddleware<GlobalException>();

            //Add Api Gateway listener
            app.UseMiddleware<ListenToOnlyApiGateway>();

            return app;
        }
    }
}
