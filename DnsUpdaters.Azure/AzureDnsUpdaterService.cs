using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using DnsUpdater.Core.Options;
using DnsUpdater.Core.Services;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.Authentication;

namespace DnsUpdater.DnsUpdaters.Azure
{
    public class AzureDnsUpdaterService : IDnsRecordUpdater
    {
        public const string ServiceKey = "AzureDns";

        private ILogger<AzureDnsUpdaterService> Logger { get; }

        private AzureDnsOptions Options { get; }

        public AzureDnsUpdaterService(ILogger<AzureDnsUpdaterService> logger, IServiceOptionsProvider serviceOptionsProvider)
        {
            Logger = logger;

            Options = serviceOptionsProvider.GetServiceOptions<AzureDnsOptions>("AzureDns");
            EnsureOptionsSet(Options);
        }

        public async Task UpdateDnsRecordAsync(IPAddress newAddress)
        {
            Logger.LogTrace("Updating {host} to {newAddress}", $"{Options.DnsRelativeRecordSetName}.{Options.DnsZoneName}",  newAddress);

            Logger.LogDebug("Silently logging in to service client");
            ServiceClientCredentials serviceClientCredentials =
                await ApplicationTokenProvider.LoginSilentAsync(
                    Options.ApplicationTenantDomain,
                    Options.ApplicationClientId,
                    Options.ApplicationClientSecret);

            var dnsManagementClient = new DnsManagementClient(serviceClientCredentials)
            {
                SubscriptionId = Options.DnsSubscriptionId
            };

            Logger.LogDebug("Fetching existing A records");
            RecordSet existingRecords =
                await dnsManagementClient.RecordSets.GetAsync(
                    Options.DnsResourceGroup,
                    Options.DnsZoneName,
                    Options.DnsRelativeRecordSetName,
                    RecordType.A);

            Logger.LogDebug("Clearing {count} existing A records", existingRecords.ARecords.Count);
            existingRecords.ARecords.Clear();
            existingRecords.ARecords.Add(new ARecord(newAddress.ToString()));

            if (Options.UpdateMetaData)
            {
                string nowStr = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                Logger.LogDebug("Updating \"LastUpdated\" metadata to {nowStr}", nowStr);
                existingRecords.Metadata ??= new Dictionary<string, string>();
                existingRecords.Metadata["LastUpdated"] = nowStr;
            }

            Logger.LogTrace("Updating A records");
            AzureOperationResponse<RecordSet> operationResponse =
                await dnsManagementClient.RecordSets.CreateOrUpdateWithHttpMessagesAsync(
                    Options.DnsResourceGroup,
                    Options.DnsZoneName,
                    Options.DnsRelativeRecordSetName,
                    RecordType.A,
                    existingRecords,
                    existingRecords.Etag);

            HttpStatusCode responseStatus = operationResponse.Response.StatusCode;
            string responseBody = await operationResponse.Response.Content.ReadAsStringAsync();
            if (operationResponse.Response.IsSuccessStatusCode)
            {
                Logger.LogDebug("Operation returned {responseStatus}: {responseBody}", responseStatus, responseBody);
                return;
            }

            Logger.LogError("Failed to update IP: {responseStatus} {responseBody}", responseStatus, responseBody);
        }

        private static void EnsureOptionsSet(AzureDnsOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("No options provided.");

            if (string.IsNullOrWhiteSpace(options.ApplicationTenantDomain))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.ApplicationTenantDomain)} must be set.");

            if (string.IsNullOrWhiteSpace(options.ApplicationClientId))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.ApplicationClientId)} must be set.");

            if (string.IsNullOrWhiteSpace(options.ApplicationClientSecret))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.ApplicationClientSecret)} must be set.");

            if (string.IsNullOrWhiteSpace(options.DnsSubscriptionId))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.DnsSubscriptionId)} must be set.");

            if (string.IsNullOrWhiteSpace(options.DnsResourceGroup))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.DnsResourceGroup)} must be set.");

            if (string.IsNullOrWhiteSpace(options.DnsZoneName))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.DnsZoneName)} must be set.");

            if (string.IsNullOrWhiteSpace(options.DnsRelativeRecordSetName))
                throw new InvalidOperationException($"{nameof(AzureDnsOptions.DnsRelativeRecordSetName)} must be set.");
        }
    }
}
