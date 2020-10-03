namespace DnsUpdater.DnsUpdaters.Azure
{
    internal class AzureDnsOptions
    {
        public string ApplicationTenantDomain { get; set; }
        public string ApplicationClientId { get; set; }
        public string ApplicationClientSecret { get; set; }

        public string DnsSubscriptionId { get; set; }
        public string DnsResourceGroup { get; set; }
        public string DnsZoneName { get; set; }
        public string DnsRelativeRecordSetName { get; set; }

        public bool UpdateMetaData { get; set; } = true;
    }
}
