using Microsoft.AspNet.SignalR.Client;
using System;
using System.Configuration;
using System.Threading;

namespace SignalRConsole
{
    public class SignalRClient
    {
        private string clientName;
        private string serverUrl;
        private HubConnection hubConnection;
        private ISignalRConfiguration Configuration;
        private IHubProxy myHubProxy;
        private Timer messageTimer;
        private Random random;
        private bool isStopping = false;
        private int maxReconnectAttempts = 1;

        public SignalRClient(string name)
        {
            Configuration = new SignalRConfiguration();
            clientName = name;
            serverUrl = Configuration.GetAppSetting("SignalRServerUrl");
            random = new Random();
            int.TryParse(Configuration.GetAppSetting("MaxReconnectAttempts"), out maxReconnectAttempts);
        }

        public void Connect()
        {
            Console.WriteLine(string.Format("Starting client\nConnecting to {0}", serverUrl));
            SetupConnection();
            SetupHubProxy();
            hubConnection.Start().Wait();
            Console.WriteLine(string.Format("Server Connected", serverUrl));
            StartTimer();
        }

        public void Send(string message)
        {
            myHubProxy.Invoke("Send", message).ContinueWith(messageTask =>
            {
                if (messageTask.IsFaulted)
                {
                    Console.WriteLine("!!! There was an error opening the connection:{0} \n", messageTask.Exception.GetBaseException());
                }

            }).Wait();
        }

        private void SetupConnection()
        {
            hubConnection = new HubConnection(serverUrl, Configuration.ConnectionParams(clientName));
            hubConnection.TransportConnectTimeout = TimeSpan.Zero;
            hubConnection.StateChanged += HubConnection_StateChanged;
            hubConnection.Reconnected += HubConnection_Reconnected;
            hubConnection.Reconnecting += HubConnection_Reconnecting;
        }

        private void SetupHubProxy()
        {
            myHubProxy = hubConnection.CreateHubProxy("ChatHub");

            myHubProxy.On<string>("broadcastMessage", (message) => Console.WriteLine(message + "\n"));
            myHubProxy.On<string>("hello", (name) => Console.WriteLine("Online Host: " + name + " has joined the chat.\n"));
            myHubProxy.On<string>("goodbye", (name) => Console.WriteLine("Online Host: " + name + " has left the chat.\n"));
            myHubProxy.On("heartbeat", () => Console.WriteLine("Recieved heartbeat \n"));
        }

        private void StartTimer()
        {
            var autoEvent = new AutoResetEvent(false);
            var startDuration = random.Next(15, 20) * 1000;
            messageTimer = new Timer(new TimerCallback(this.MessageTimerCallback), autoEvent, startDuration, 0);
        }

        private void MessageTimerCallback(Object stateInfo)
        {
            var now = DateTime.Now.ToString();
            this.Send(string.Format("Hello at {0}", now));
            var newDuration = random.Next(15, 20) * 1000;
            messageTimer.Change(newDuration, 0);
        }

        private void HubConnection_Reconnecting()
        {
            Console.WriteLine("Online host: Chat reconnecting");
        }

        private void HubConnection_Reconnected()
        {
            Console.WriteLine("Online host: Server Reconnected\n\tWelcome to the chat");
        }

        private void HubConnection_StateChanged(StateChange obj)
        {
            if (obj.NewState == ConnectionState.Connected)
            {
                Console.WriteLine("Online host: Welcome to the chat");
            }
            else if (obj.NewState == ConnectionState.Disconnected)
            {
                Reconnect();
            }
            else if (obj.NewState == ConnectionState.Connecting)
            {
                Console.WriteLine("Online host: Chat connecting");
            }
        }

        private void Reconnect()
        {
            if (!isStopping)
            {
                Reconnect(maxReconnectAttempts);
            }
        }

        private void Reconnect(int reconnects)
        {
            if (hubConnection.State != ConnectionState.Connected)
            {
                Console.WriteLine("Online host: Chat disconnected.\n\tAttempting reconnect in 30 seconds.");
                messageTimer.Dispose();
                hubConnection.Dispose();
                Thread.Sleep(30000);
                try
                {
                    Connect();
                }
                catch (Exception ex)
                {
                    if (reconnects > 0)
                    {
                        Reconnect(reconnects - 1);
                    }
                    else
                    {
                        Console.WriteLine("Online host: Max number of reconnects attenpted.");
                    }
                }
            }

        }

        public void Disconnect()
        {
            isStopping = true;
            messageTimer.Dispose();
            hubConnection.Stop();
            hubConnection.Dispose();
        }
    }
}
