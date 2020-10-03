namespace DnsUpdater.Host
{
    /// <summary>
    ///     Options for the <see cref="DnsService"/>.
    /// </summary>
    internal class DnsHostOptions
    {
        /// <summary>
        ///     How often to check the IP in milliseconds.
        /// </summary>
        public int CheckIntervalMs { get; set; }
    }
}
