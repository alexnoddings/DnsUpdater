namespace DnsUpdater.Core.Options
{
    public interface IServiceOptionsProvider
    {
        public TConfig GetServiceOptions<TConfig>(string serviceNameKey);
    }
}
