using DnsUpdater.Core.Options;
using Microsoft.Extensions.DependencyInjection;

namespace DnsUpdater.Core.Extensions
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
