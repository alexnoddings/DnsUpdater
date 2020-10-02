using Microsoft.Extensions.Configuration;

namespace DnsUpdater.Core.Options
{
    internal class ServiceOptionsProvider : IServiceOptionsProvider
    {
        private IConfiguration Configuration { get; }

        public ServiceOptionsProvider(IConfiguration configuration) =>
            Configuration = configuration;

        public TOptions GetServiceOptions<TOptions>(string serviceNameKey) =>
            Configuration
                .GetSection("Services")
                .GetSection(serviceNameKey)
                .Get<TOptions>();
    }
}
