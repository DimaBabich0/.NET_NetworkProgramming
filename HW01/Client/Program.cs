using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private static bool canSendOrder = true;

        static void Main()
        {
            Console.Title = "Client";
            try
            {
                client = new TcpClient("127.0.0.1", 4900);
                stream = client.GetStream();
                Console.WriteLine("Connected to server");

                Thread receiverThread = new Thread(Receiver);
                receiverThread.Start();
                Thread.Sleep(100);
                Thread senderThread = new Thread(Sender);
                senderThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void Sender()
        {
            while (true)
            {
                if (canSendOrder)
                {
                    Console.Write("Enter your order: ");
                    string query = Console.ReadLine();

                    byte[] data = Encoding.UTF8.GetBytes(query);
                    stream.Write(data, 0, data.Length);
                    canSendOrder = false;
                }
            }
        }

        private static void Receiver()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (response == "stop")
                    canSendOrder = false;
                else if (response == "begin")
                    canSendOrder = true;
                else
                    Console.WriteLine($"\n{response}\n");
            }
        }
    }
}
