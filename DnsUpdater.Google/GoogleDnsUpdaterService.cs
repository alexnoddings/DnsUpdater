using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Options;
using Core.Services;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.Google
{
    internal class GoogleDnsUpdaterService : IDnsRecordUpdater, IDisposable
    {
        private const string ApiEndpointFormat = "https://@domains.google.com/nic/update?hostname={0}&myip={1}";

        private ILogger<GoogleDnsUpdaterService> Logger { get; }

        private GoogleDnsOptions Options { get; }
        private HttpClient HttpClient { get; }

        public GoogleDnsUpdaterService(ILogger<GoogleDnsUpdaterService> logger, IServiceOptionsProvider serviceOptionsProvider)
        {
            Logger = logger;

            Options = serviceOptionsProvider.GetServiceOptions<GoogleDnsOptions>("GoogleDns");
            EnsureOptionsSet(Options);

            var credentials = new NetworkCredential(Options.Username, Options.Password);
            var handler = new HttpClientHandler { Credentials = credentials };
            HttpClient = new HttpClient(handler, true);
        }

        public async Task UpdateDnsRecordAsync(IPAddress newAddress)
        {
            Logger.LogInformation($"Updating {Options.Hostname} to {newAddress}");

            string requestUri = string.Format(ApiEndpointFormat, Options.Hostname, newAddress);
            HttpResponseMessage response = await HttpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode) return;

            HttpStatusCode status = response.StatusCode;
            string body = await response.Content.ReadAsStringAsync();
            Logger.LogError("Failed up update IP: {status} {body}", status, body);
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
