using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Core.Interfaces
{
    public interface IDnsUpdaterService
    {
        void Start();
        void Stop();
        void OnElapsedTime(object source, ElapsedEventArgs e);
        string GetWebResponse(string url);
        string GetIp();
        string UpdateIp(string newIp);
    }
}
