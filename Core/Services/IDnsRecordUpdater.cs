using System.Net;
using System.Threading.Tasks;

namespace DnsUpdater.Core.Services
{
    public interface IDnsRecordUpdater
    {
        public Task UpdateDnsRecordAsync(IPAddress newAddress);
    }
}
