using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    internal class Order
    {
        public TcpClient Client { get; }
        public float Price { get; }
        public int Time { get; }
        public string Message { get; }

        public Order(TcpClient client, float price, int time, string message)
        {
            Client = client;
            Price = price;
            Time = time;
            Message = message;
        }
    }

    internal class OrderThread
    {
        public List<Order> Orders { get; } = new List<Order>();
        private Thread thread;

        public void StartThread()
        {
            thread = new Thread(ProcessOrders);
            thread.Start();
        }

        private void ProcessOrders()
        {
            while (true)
            {
                if (Orders.Count > 0)
                {
                    Order order = Orders[0];
                    Thread.Sleep(order.Time * 1000);

                    try
                    {
                        NetworkStream stream = order.Client.GetStream();
                        byte[] message = Encoding.UTF8.GetBytes("Your order is ready. Enjoy your meal.");
                        stream.Write(message, 0, message.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending message: {ex.Message}");
                    }

                    Orders.RemoveAt(0);
                }
            }
        }
    }
}
