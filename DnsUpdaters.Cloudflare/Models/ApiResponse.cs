using System.Collections.Generic;

namespace DnsUpdater.DnsUpdaters.Cloudflare.Models
{
    internal class ApiResponse<TResult>
    {
        public List<TResult> Result { get; set; }
    }
}
