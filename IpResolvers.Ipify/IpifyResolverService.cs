using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsUpdater.Core.Services;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.IpResolvers.Ipify
{
    public class IpifyResolverService : IIpAddressResolver
    {
        public const string ServiceKey = "IpifyResolver";

        private const string ApiEndpoint = "https://api.ipify.org";

        private ILogger<IpifyResolverService> Logger { get; }

        private HttpClient HttpClient { get; } = new();

        public IpifyResolverService(ILogger<IpifyResolverService> logger)
        {
            Logger = logger;
        }

        public async Task<IPAddress> GetCurrentIpAddressAsync()
        {
            Logger.LogTrace("Resolving current IP");

            HttpResponseMessage response = await HttpClient.GetAsync(ApiEndpoint);
            Logger.LogDebug("Fetching API returned a {statusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                HttpStatusCode responseStatus = response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response status {responseStatus} did not indicate a successful response: {responseBody}");
            }

            string body = await response.Content.ReadAsStringAsync();
            Logger.LogTrace("Ipify API response: \"{body}\"", body);

            IPAddress ipAddress = IPAddress.Parse(body);
            Logger.LogTrace("Current IP is {ipAddress}", ipAddress);

            return ipAddress;
        }
    }
}
