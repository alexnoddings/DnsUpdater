using System.Net;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IIpAddressResolver
    {
        public Task<IPAddress?> GetCurrentIpAddressAsync();
    }
}
