using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
namespace KinneySignalRServer
{
    public class ChatHub : Hub
    {
        private Dictionary<string, string> connectionIdToName = new Dictionary<string, string>();

        public void Send(string message)
        {
            Debug.WriteLine("Hub OnConnected {0}\n", Context.ConnectionId);
            string name = Context.QueryString.Get("name");
            Clients.All.broadcastMessage(string.Format("{0}: {1}", name, message));
        }

        public override Task OnConnected()
        {
            Debug.WriteLine("Hub OnConnected {0}\n", Context.ConnectionId);
            string name = Context.QueryString.Get("name");
            Clients.All.hello(name);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Debug.WriteLine("Hub OnDisconnected {0}\n", Context.ConnectionId);
            string name = Context.QueryString.Get("name");;
            Clients.All.goodbye(name);
            return base.OnDisconnected(stopCalled);
        }
    }
}