using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DnsUpdater.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.Host
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
            // Stops the "Application started" log appearing after we have started
            await Task.Delay(100);
            Logger.LogInformation("Starting with {checkInterval}ms check interval.", Options.CheckIntervalMs);

            bool isFirstRequest = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (isFirstRequest)
                    isFirstRequest = false;
                else
                    await Task.Delay(Options.CheckIntervalMs, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                using var scope = ScopeFactory.CreateScope();

                IPAddress ip;
                Logger.LogDebug("Fetching current IP");
                try
                {
                    var ipResolver = scope.ServiceProvider.GetRequiredService<IIpAddressResolver>();
                    ip = await ipResolver.GetCurrentIpAddressAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to resolve IP, skipping this cycle.");
                    continue;
                }

                if (!ip.Equals(LastKnownIp))
                {
                    Logger.LogInformation("IP has changed to {ip}, updating.", ip);

                    try
                    {
                        var updater = scope.ServiceProvider.GetRequiredService<IDnsRecordUpdater>();
                        await updater.UpdateDnsRecordAsync(ip);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Failed to update IP.");
                        continue;
                    }

                    Logger.LogInformation("IP updated.");
                    LastKnownIp = ip;
                }
                else
                {
                    Logger.LogDebug("Current IP has not changed.");
                }
            }
            Logger.LogInformation("Cancellation requested, stopping service.");
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
