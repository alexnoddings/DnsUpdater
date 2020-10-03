using DnsUpdater.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DnsUpdater.DnsUpdaters.Azure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureDnsUpdaterService(this IServiceCollection services)
        {
            services.AddScoped<IDnsRecordUpdater, AzureDnsUpdaterService>();

            return services;
        }
    }
}
