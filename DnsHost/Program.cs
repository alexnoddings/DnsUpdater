using Core.Extensions;
using DnsUpdater.Google;
using IpResolvers.Ipify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DnsHost
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddServiceOptionsProvider();

                    services.AddIpifyResolverService();
                    services.AddGoogleDnsUpdaterService();

                    services.AddHostedService<DnsService>();
                });
    }
}
