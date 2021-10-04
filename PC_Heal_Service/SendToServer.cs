using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PC_Heal_Service
{
    class SendToServer
    {
        public static void Send(CI computerInfor)
        {
            var serverIP = IPAddress.Parse("127.0.0.1");
            int serverPort = 5000;

            while (true)
            {
                var client = new TcpClient();
                client.Connect(serverIP, serverPort);
                var localEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
                Console.WriteLine("Connecting to " + localEndpoint.Address + ":" + localEndpoint.Port);

                var stream = client.GetStream();
                var writer = new StreamWriter(stream) { AutoFlush = true };

                var computerInfo = computerInfor;
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, computerInfo);

            }
        }
    }
}
