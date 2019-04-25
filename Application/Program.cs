using Core;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    class Program
    {
        private static IDnsUpdaterService _updater;
        static void Main(string[] args)
        {
            _updater = new DnsUpdaterService();
            _updater.Start();
        }
    }
}
