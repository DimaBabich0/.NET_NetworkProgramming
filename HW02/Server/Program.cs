using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        private static List<TcpClient> clients = new List<TcpClient>();
        private static TcpListener server;
        private static List<string> quotes = new List<string>();

        static void Main()
        {
            LoadWordsFromFile("Quotes.txt");

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

        private static void LoadWordsFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                quotes = new List<string>(File.ReadAllLines(fileName));
                Console.WriteLine($"Quotes are loaded from file {fileName}");
            }
            else
            {
                Console.WriteLine($"File {fileName} not found. Use quotes from the program");
                quotes = new List<string>
                {
                    "Hey everyone!",
                    "What’s up?",
                    "BRB",
                    "That’s awesome!",
                    "No worries!",
                    "Sounds good!",
                    "See you later!"
                };
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Request from client: {request}");

                    if (request == "GET_QUOTE")
                    {
                        Random r = new Random();
                        string randomWord = quotes[r.Next(quotes.Count)];
                        byte[] data = Encoding.UTF8.GetBytes(randomWord);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine($"Quote sent: {randomWord}");
                    }
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
    }
}
