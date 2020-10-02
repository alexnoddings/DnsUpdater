using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DnsHost
{
    internal class DnsService : BackgroundService
    {
        private ILogger<DnsService> Logger { get; }

        private IServiceScopeFactory ScopeFactory { get; }
        private IPAddress? LastKnownIp { get; set; }

        private DnsHostOptions Options { get; }

        public DnsService(ILogger<DnsService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            Logger = logger;
            ScopeFactory = scopeFactory;

            using var scope = scopeFactory.CreateScope();
            Options = configuration.GetSection("Host").Get<DnsHostOptions>();
            EnsureOptionsSet(Options);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Starting");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var ipResolver = scope.ServiceProvider.GetRequiredService<IIpAddressResolver>();
                    IPAddress? ip = null;
                    var ipResolverFailed = false;
                    try
                    {
                        ip = await ipResolver.GetCurrentIpAddressAsync();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Failed to resolve IP, skipping this cycle:\n{e}", e);
                        ipResolverFailed = true;
                    }

                    if (!ipResolverFailed)
                    {
                        if (ip == null)
                        {
                            Logger.LogWarning("IP resolver failed to return an IP, skipping this cycle");
                        }
                        else if (!ip.Equals(LastKnownIp))
                        {
                            Logger.LogInformation("New IP is {ip}, updating", ip);
                            LastKnownIp = ip;
                            var updater = scope.ServiceProvider.GetRequiredService<IDnsRecordUpdater>();
                            try
                            {
                                await updater.UpdateDnsRecordAsync(ip);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Failed to update IP:\n{e}", e);
                            }
                        }
                    }
                }
                await Task.Delay(Options.CheckIntervalMs, stoppingToken);
            }
            Logger.LogInformation("Cancellation requested, stopping service");
        }

        private static void EnsureOptionsSet(DnsHostOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("No options provided.");

            if (options.CheckIntervalMs < 1000)
                throw new InvalidOperationException($"{nameof(DnsHostOptions.CheckIntervalMs)} must be >= 1000.");
        }
    }
}
