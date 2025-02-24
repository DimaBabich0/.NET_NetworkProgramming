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
    struct Dish
    {
        public string Name { get; }
        public float Price { get; }
        public int Time { get; }

        public Dish(string name, float price, int time)
        {
            Name = name;
            Price = price;
            Time = time;
        }

        public override string ToString()
        {
            return $"Dish: {Name}; Price: {Price:F2} UAH; Time to cook: {Time} sec";
        }
    }

    class Program
    {
        private static List<TcpClient> clients = new List<TcpClient>();
        private static TcpListener server;
        private static List<Dish> dishes = new List<Dish>();
        private static OrderThread orderThread = new OrderThread();

        static void Main()
        {
            Console.Title = "Server";
            LoadDishes();

            orderThread.StartThread();
            server = new TcpListener(IPAddress.Any, 4900);
            server.Start();
            Console.WriteLine("Server is running");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client);
                Console.WriteLine("New client connected");

                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            string menu = "\tMenu:\n";
            foreach (Dish d in dishes)
            {
                menu += d + "\n"; 
            }
            SendMessage(stream, menu);

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).ToLower();
                    Console.WriteLine($"Request from client: {request}");

                    float totalPrice = 0;
                    int totalTime = 0;

                    foreach (var dish in dishes)
                    {
                        if (request.Contains(dish.Name.ToLower()))
                        {
                            totalPrice += dish.Price;
                            totalTime += dish.Time;
                        }
                    }

                    if (totalTime == 0)
                    {
                        SendMessage(stream, "Sorry, but you wrote a wrong order. Try again");
                        SendMessage(stream, "begin");
                        continue;
                    }

                    string orderMessage = $"Your order is accepted\nOrder cost: {totalPrice:F2} UAH\nCooking time: {totalTime} seconds\nPlease wait";
                    SendMessage(stream, orderMessage);
                    orderThread.Orders.Add(new Order(client, totalPrice, totalTime, orderMessage));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client error: " + ex.Message);
            }
            finally
            {
                clients.Remove(client);
                client.Close();
                Console.WriteLine("Client disconnected");
            }
        }

        static void LoadDishes()
        {
            dishes.Add(new Dish("Big Mac", 119.99f, 20));
            dishes.Add(new Dish("Cheeseburger", 68.99f, 8));
            dishes.Add(new Dish("Double Cheeseburger", 99.99f, 12));
            dishes.Add(new Dish("Fries", 80.59f, 6));
            dishes.Add(new Dish("Coca-Cola", 34.98f, 3));
            dishes.Add(new Dish("Sprite", 34.98f, 3));
        }

        static void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}
