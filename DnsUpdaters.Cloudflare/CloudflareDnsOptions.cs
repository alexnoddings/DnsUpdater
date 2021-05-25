namespace DnsUpdater.DnsUpdaters.Cloudflare
{
    internal class CloudflareDnsOptions
    {
        public string Zone { get; set; }
        public string DnsRecord { get; set; }

        public string Key { get; set; }
    }
}
