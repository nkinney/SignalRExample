using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRConsole
{
    public class SignalRConfiguration : ISignalRConfiguration
    {
        public string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public string ConnectionParams(string clientName)
        {
            return string.Format("name={0}", clientName);
        }
    }
}
