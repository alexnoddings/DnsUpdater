using System;
using System.Collections.Generic;
using DnsUpdater.Core.Extensions;
using DnsUpdater.Core.Services;
using DnsUpdater.DnsUpdaters.Azure;
using DnsUpdater.DnsUpdaters.Cloudflare;
using DnsUpdater.DnsUpdaters.Google;
using DnsUpdater.IpResolvers.Ipify;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.Host
{
    /// <summary>
    ///     Host entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     Dictionary of DnsRecordUpdater keys and their corresponding types.
        /// </summary>
        private static IDictionary<string, Type> DnsUpdaters { get; } = new Dictionary<string, Type>
        {
            {AzureDnsUpdaterService.ServiceKey, typeof(AzureDnsUpdaterService)},
            {CloudflareDnsUpdaterService.ServiceKey, typeof(CloudflareDnsUpdaterService)},
            {GoogleDnsUpdaterService.ServiceKey, typeof(GoogleDnsUpdaterService)}
        };

        /// <summary>
        ///     Dictionary of IpAddressResolver keys and their corresponding types.
        /// </summary>
        private static IDictionary<string, Type> IpAddressResolvers { get; } = new Dictionary<string, Type>
        {
            {IpifyResolverService.ServiceKey, typeof(IpifyResolverService)}
        };

        /// <summary>
        ///     Program entry point.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments.
        /// </param>
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        /// <summary>
        ///     Creates an <see cref="IHostBuilder"/> for the program.
        /// </summary>
        /// <param name="args">
        ///     Arguments to create the host with.
        /// </param>
        /// <returns>
        ///     An <see cref="IHostBuilder"/>.
        /// </returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddServiceOptionsProvider();

                    services.AddScoped<IIpAddressResolver>(CreateIpAddressResolver);
                    services.AddScoped<IDnsRecordUpdater>(CreateDnsRecordUpdater);

                    services.AddScoped<IpifyResolverService>();

                    services.AddScoped<AzureDnsUpdaterService>();
                    services.AddScoped<CloudflareDnsUpdaterService>();
                    services.AddScoped<GoogleDnsUpdaterService>();

                    services.AddHostedService<DnsService>();
                });

        /// <summary>
        ///     Creates a <see cref="IIpAddressResolver"/> from the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceProvider"/> to get the <see cref="IIpAddressResolver"/> from.
        /// </param>
        /// <returns>
        ///     The created <see cref="IIpAddressResolver"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the registered service is not of type <see cref="IIpAddressResolver"/>.
        /// </exception>
        private static IIpAddressResolver CreateIpAddressResolver(IServiceProvider services)
        {
            IConfiguration config = services.GetRequiredService<IConfiguration>();
            string ipResolverServiceKey = config["Host:IpResolverKey"];

            if (!IpAddressResolvers.ContainsKey(ipResolverServiceKey))
            {
                string badKeyMessage = $"IpResolver key \"{ipResolverServiceKey}\" is not valid.";
                services.GetService<ILogger<Program>>()?.LogCritical(badKeyMessage);
                throw new InvalidOperationException(badKeyMessage);
            }

            Type serviceType = IpAddressResolvers[ipResolverServiceKey];
            if (services.GetService(serviceType) is IIpAddressResolver service)
                return service;

            string message = $"{nameof(IIpAddressResolver)} type {serviceType.Name} was not registered properly.";
            services.GetService<ILogger<Program>>()?.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        /// <summary>
        ///     Creates a <see cref="IDnsRecordUpdater"/> from the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceProvider"/> to get the <see cref="IDnsRecordUpdater"/> from.
        /// </param>
        /// <returns>
        ///     The created <see cref="IDnsRecordUpdater"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the registered service is not of type <see cref="IDnsRecordUpdater"/>.
        /// </exception>
        private static IDnsRecordUpdater CreateDnsRecordUpdater(IServiceProvider services)
        {
            IConfiguration config = services.GetRequiredService<IConfiguration>();
            string dnsRecordUpdaterServiceKey = config["Host:DnsUpdaterKey"];

            if (!DnsUpdaters.ContainsKey(dnsRecordUpdaterServiceKey))
            {
                string badKeyMessage = $"DnsUpdater key \"{dnsRecordUpdaterServiceKey}\" is not valid.";
                services.GetService<ILogger<Program>>()?.LogCritical(badKeyMessage);
                throw new InvalidOperationException(badKeyMessage);
            }

            Type serviceType = DnsUpdaters[dnsRecordUpdaterServiceKey];
            if (services.GetService(serviceType) is IDnsRecordUpdater service)
                return service;

            string message = $"{nameof(IDnsRecordUpdater)} type {serviceType.Name} was not registered properly.";
            services.GetService<ILogger<Program>>()?.LogCritical(message);
            throw new InvalidOperationException(message);
        }
    }
}
