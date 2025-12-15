using AutoNexus.Application.Interfaces;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Infrastructure.ExternalServices;
using AutoNexus.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNexus.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<SoftDeleteInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<SoftDeleteInterceptor>();

                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .AddInterceptors(interceptor);
            });

            services.AddHttpClient<IFipeService, FipeService>();
            services.AddHttpClient<IAddressService, ViaCepService>();

            return services;
        }
    }
}