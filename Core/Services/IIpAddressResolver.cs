using System.Net;
using System.Threading.Tasks;

namespace DnsUpdater.Core.Services
{
    public interface IIpAddressResolver
    {
        public Task<IPAddress> GetCurrentIpAddressAsync();
    }
}
