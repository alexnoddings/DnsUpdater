using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DnsUpdater.Core.Options;
using DnsUpdater.Core.Services;
using DnsUpdater.DnsUpdaters.Cloudflare.Models;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.DnsUpdaters.Cloudflare
{
    public class CloudflareDnsUpdaterService : IDnsRecordUpdater
    {
        public const string ServiceKey = "CloudflareDns";

        private const string ZoneApiEndpointBase = "https://api.cloudflare.com/client/v4/zones";
        private const string ZoneIdApiEndpointFormat = ZoneApiEndpointBase + "?name={0}&status=active";
        private const string DnsRecordIdApiEndpointFormat = ZoneApiEndpointBase + "/{0}/dns_records?type=A&name={1}";
        private const string DnsRecordUpdateApiEndpointFormat = ZoneApiEndpointBase + "/{0}/dns_records/{1}";

        private ILogger<CloudflareDnsUpdaterService> Logger { get; }

        private CloudflareDnsOptions Options { get; }
        private HttpClient HttpClient { get; }

        public CloudflareDnsUpdaterService(ILogger<CloudflareDnsUpdaterService> logger, IServiceOptionsProvider serviceOptionsProvider)
        {
            Logger = logger;

            Options = serviceOptionsProvider.GetServiceOptions<CloudflareDnsOptions>(ServiceKey);
            EnsureOptionsSet(Options);

            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Options.Key}");
        }

        public async Task UpdateDnsRecordAsync(IPAddress newAddress)
        {
            var zoneRequestUri = string.Format(ZoneIdApiEndpointFormat, Options.Zone);
            Logger.LogDebug("Getting zone.");
            var zoneResponse = await HttpClient.GetFromJsonAsync<ApiResponse<Zone>>(zoneRequestUri);

            var zoneId = zoneResponse?.Result?.FirstOrDefault()?.Id;
            if (zoneId is null)
            {
                Logger.LogCritical("Could not locate id for zone.");
                return;
            }

            var dnsRecordRequestUri = string.Format(DnsRecordIdApiEndpointFormat, zoneId, Options.DnsRecord);
            Logger.LogDebug("Getting current DNS record.");
            var dnsRecordResponse = await HttpClient.GetFromJsonAsync<ApiResponse<DnsRecord>>(dnsRecordRequestUri);

            var dnsRecord = dnsRecordResponse?.Result?.FirstOrDefault();
            if (dnsRecord is null)
            {
                Logger.LogCritical("Could not locate id for DNS record.");
                return;
            }

            var dnsRecordUpdateRequestUri = string.Format(DnsRecordUpdateApiEndpointFormat, Options.Zone, dnsRecord.Id);
            var newDnsRecord = new DnsRecord
            {
                Id = dnsRecord.Id,
                Type = dnsRecord.Type,
                Name = dnsRecord.Name,
                Content = newAddress.ToString(),
                Ttl = dnsRecord.Ttl,
                Proxied = dnsRecord.Proxied
            };
            Logger.LogDebug("Updating DNS record.");
            await HttpClient.PutAsJsonAsync(dnsRecordUpdateRequestUri, newDnsRecord);
        }

        private static void EnsureOptionsSet(CloudflareDnsOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("No options provided.");

            if (string.IsNullOrWhiteSpace(options.Zone))
                throw new InvalidOperationException($"{nameof(CloudflareDnsOptions.Zone)} must be set.");

            if (string.IsNullOrWhiteSpace(options.DnsRecord))
                throw new InvalidOperationException($"{nameof(CloudflareDnsOptions.DnsRecord)} must be set.");

            if (string.IsNullOrWhiteSpace(options.Key))
                throw new InvalidOperationException($"{nameof(CloudflareDnsOptions.Key)} must be set.");
        }
    }
}
