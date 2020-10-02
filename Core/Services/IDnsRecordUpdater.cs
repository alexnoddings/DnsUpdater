using System.Net;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IDnsRecordUpdater
    {
        public Task UpdateDnsRecordAsync(IPAddress newAddress);
    }
}
