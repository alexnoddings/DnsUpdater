using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsUpdater.Core.Options;
using DnsUpdater.Core.Services;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.DnsUpdaters.Google
{
    public class GoogleDnsUpdaterService : IDnsRecordUpdater, IDisposable
    {
        public const string ServiceKey = "GoogleDns";

        private const string ApiEndpointFormat = "https://@domains.google.com/nic/update?hostname={0}&myip={1}";

        private ILogger<GoogleDnsUpdaterService> Logger { get; }

        private GoogleDnsOptions Options { get; }
        private HttpClient HttpClient { get; }

        public GoogleDnsUpdaterService(ILogger<GoogleDnsUpdaterService> logger, IServiceOptionsProvider serviceOptionsProvider)
        {
            Logger = logger;

            Options = serviceOptionsProvider.GetServiceOptions<GoogleDnsOptions>(ServiceKey);
            EnsureOptionsSet(Options);

            var credentials = new NetworkCredential(Options.Username, Options.Password);
            var handler = new HttpClientHandler { Credentials = credentials };
            HttpClient = new HttpClient(handler, true);
        }

        public async Task UpdateDnsRecordAsync(IPAddress newAddress)
        {
            Logger.LogTrace("Updating {host} to {newAddress}", Options.Hostname, newAddress);

            string requestUri = string.Format(ApiEndpointFormat, Options.Hostname, newAddress);

            HttpResponseMessage response = await HttpClient.GetAsync(requestUri);

            HttpStatusCode responseStatus = response.StatusCode;
            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Logger.LogDebug("Operation returned a {responseStatus}: {responseBody}", responseStatus, responseBody);  
            }

            Logger.LogError("Failed up update IP: {status} {body}", responseStatus, responseBody);
        }

        private static void EnsureOptionsSet(GoogleDnsOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("No options provided.");

            if (string.IsNullOrWhiteSpace(options.Username))
                throw new InvalidOperationException($"{nameof(GoogleDnsOptions.Username)} must be set.");

            if (string.IsNullOrWhiteSpace(options.Password))
                throw new InvalidOperationException($"{nameof(GoogleDnsOptions.Password)} must be set.");

            if (string.IsNullOrWhiteSpace(options.Hostname))
                throw new InvalidOperationException($"{nameof(GoogleDnsOptions.Hostname)} must be set.");
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
