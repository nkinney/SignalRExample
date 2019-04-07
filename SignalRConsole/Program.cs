using System;

namespace SignalRConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientName = "";
            do
            {
                Console.WriteLine("Please enter your name");
                clientName = Console.ReadLine();
            } while (clientName == "");
            var signalRClient = new SignalRClient(clientName);
            signalRClient.Connect();
            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }
                else
                {
                    signalRClient.Send(message);
                }
            }
            signalRClient.Disconnect();
        }
    }
}