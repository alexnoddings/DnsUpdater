using Core;
using Core.Interfaces;
using System.ServiceProcess;

namespace Google_DNS_Updater
{
    public partial class DnsService : ServiceBase
    {

        private IDnsUpdaterService _updater;
        public DnsService()
        {
            InitializeComponent();
            _updater = new DnsUpdaterService();
        }

        protected override void OnStart(string[] args)
        {
            _updater.Start();
        }

        protected override void OnStop()
        {
            _updater.Stop();
        }

    }
}
