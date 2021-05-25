namespace DnsUpdater.DnsUpdaters.Cloudflare.Models
{
    internal class DnsRecord
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public bool Proxied { get; set; }
        public int Ttl { get; set; }
    }
}
