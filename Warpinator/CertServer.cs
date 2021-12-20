using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Warpinator
{
    class CertServer
    {
        public const string Request = "REQUEST";
        static int Port;

        static UdpClient client;
        static Thread serverThread;
        static bool running = false;

        public static void Start(int port)
        {
            Port = port;
            if (client != null)
                client.Close();
            running = true;
            serverThread = new Thread(() => Run());
            serverThread.Start();
        }

        public static void Stop()
        {
            running = false;
            if (client != null)
                client.Close();
        }

        private static void Run()
        {
            client = new UdpClient(Port, AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(0, 0);

            byte[] sendData = Authenticator.GetBoxedCertificate();
            sendData = Encoding.ASCII.GetBytes(Convert.ToBase64String(sendData));
            while (running)
            {
                try
                {
                    byte[] data = client.Receive(ref endPoint);
                    string request = Encoding.UTF8.GetString(data);
                    if (request == Request)
                    {
                        client.Send(sendData, sendData.Length, endPoint);
                        Console.WriteLine($"Certificate sent to {endPoint}");
                    }
                }
                catch (Exception e)
                {
                    if (running)
                        Console.WriteLine("Error while running certserver. Restarting. Exception: " + e.Message);
                }
            }
        }
    }
}
