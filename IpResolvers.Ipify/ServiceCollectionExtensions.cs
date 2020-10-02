using DnsUpdater.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DnsUpdater.IpResolvers.Ipify
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIpifyResolverService(this IServiceCollection services)
        {
            services.AddScoped<IIpAddressResolver, IpifyResolverService>();

            return services;
        }
    }
}
