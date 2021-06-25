using System.Collections.Generic;

namespace DnsUpdater.DnsUpdaters.Cloudflare.Models
{
    internal class ErrorResponse
    {
        public List<Error> Errors { get; set; }
    }
}
