using Core.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceOptionsProvider(this IServiceCollection services)
        {
            services.AddScoped<IServiceOptionsProvider, ServiceOptionsProvider>();

            return services;
        }
    }
}
