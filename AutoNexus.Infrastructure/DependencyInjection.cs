using AutoNexus.Application.Interfaces;
using AutoNexus.Domain;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Infrastructure.ExternalServices;
using AutoNexus.Infrastructure.Persistence.Interceptors;
using AutoNexus.Infrastructure.Services;
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

            services.AddHttpClient<IFipeService, FipeService>(client =>
            {
                client.BaseAddress = new Uri(Constants.FIPE_URL);
            });

            services.AddScoped<ISaleService, SaleService>();

            services.AddHttpClient<GeminiService>();

            return services;
        }
    }
}