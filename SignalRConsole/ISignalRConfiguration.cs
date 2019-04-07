using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRConsole
{
    public interface ISignalRConfiguration
    {
        string GetAppSetting(string key);
        string ConnectionParams(string clientName);
    }
}
