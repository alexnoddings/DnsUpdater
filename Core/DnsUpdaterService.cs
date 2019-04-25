using Core.Interfaces;
using Core.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Timers;

namespace Core
{
    public class DnsUpdaterService : IDnsUpdaterService
    {
        private Timer _timer;
        private string _currentIp;
        private readonly Credentials _credentials = new Credentials();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public DnsUpdaterService()
        {
            _timer = new Timer();
            _credentials = LoadCredentialsFromJson();
        }


        public void Start()
        {
            logger.Info("DNS-Updater Starting");

            _currentIp = GetIp();
            logger.Info("Initial IP: {0}", _currentIp);
            logger.Info(UpdateIp(_currentIp));

            _timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            _timer.Interval = 120000;
            _timer.Enabled = true;
        }

        public void Stop()
        {
            logger.Warn("DNS-Updater has stopped");
            NLog.LogManager.Shutdown();
        }

        public void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            var checkIp = GetIp();
            if (checkIp != _currentIp)
            {
                logger.Info("IP has changed: {0} -> {1}", _currentIp, checkIp);
                _currentIp = checkIp;
                logger.Info("Response from IP update: {0}", GetWebResponse(UpdateIp(checkIp)));
            }
        }

        public string GetWebResponse(string url)
        {
            var request= HttpWebRequest.Create(url);
            request.Credentials = new NetworkCredential(_credentials.Username, _credentials.Password);

            Stream response = request.GetResponse().GetResponseStream();

            using (Stream stream = response)
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public string GetIp()
        {
            return GetWebResponse("https://domains.google.com/checkip");
        }

        public string UpdateIp(string newIp)
        {
            var url = "https://@domains.google.com/nic/update?hostname=" + _credentials.Domain + "&myip=" + newIp;
            return GetWebResponse(url);
        }

        private Credentials LoadCredentialsFromJson()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + "\\Config";
            string file = "config.json";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists($"{directory}\\{file}"))
            {
                logger.Error("[Error] {0} does not exist, generating config.json", file);

                File.WriteAllText($"{directory}\\{file}", JsonConvert.SerializeObject(_credentials));
                Environment.Exit(-1);
                  
            }
            var credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\config.json"));

            if(credentials == null || credentials.Username == null || credentials.Password == null || credentials.Domain == null)
            {
                logger.Error("[Error] {0} does not contain valid credentials", file);
                Environment.Exit(-1);
            }

            return credentials;
        }

    }
}
