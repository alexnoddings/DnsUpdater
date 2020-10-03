using DnsUpdater.Core.Extensions;
using DnsUpdater.DnsUpdaters.Azure;
using DnsUpdater.IpResolvers.Ipify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DnsUpdater.Host
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddServiceOptionsProvider();

                    services.AddIpifyResolverService();
                    //services.AddGoogleDnsUpdaterService();
                    services.AddAzureDnsUpdaterService();

                    services.AddHostedService<DnsService>();
                });
    }
}
