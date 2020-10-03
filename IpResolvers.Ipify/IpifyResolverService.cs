using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsUpdater.Core.Services;
using Microsoft.Extensions.Logging;

namespace DnsUpdater.IpResolvers.Ipify
{
    internal class IpifyResolverService : IIpAddressResolver
    {
        private const string ApiEndpoint = "https://api.ipify.org";

        private ILogger<IpifyResolverService> Logger { get; }

        private HttpClient HttpClient { get; } = new HttpClient();

        public IpifyResolverService(ILogger<IpifyResolverService> logger)
        {
            Logger = logger;
        }

        public async Task<IPAddress?> GetCurrentIpAddressAsync()
        {
            HttpResponseMessage response = await HttpClient.GetAsync(ApiEndpoint);

            if (!response.IsSuccessStatusCode)
            {
                HttpStatusCode status = response.StatusCode;
                string errorBody = await response.Content.ReadAsStringAsync();
                Logger.LogError($"Failed up fetch IP: {status} {errorBody}");
                return null;
            }

            string body = await response.Content.ReadAsStringAsync();
            if (!IPAddress.TryParse(body, out IPAddress? ipAddress))
            {
                Logger.LogError($"Could not parse response: {body}");
                return null;
            }

            return ipAddress;
        }
    }
}
