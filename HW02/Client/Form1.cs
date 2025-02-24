using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client?.Close();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 4900);
                stream = client.GetStream();
                OutputText("Connected to server");
            }
            catch (Exception ex)
            {
                OutputText($"Error: {ex.Message}");
                textBoxMessage.Enabled = false;
                btnSend.Enabled = false;
            }
        }

        private void SendMessage()
        {
            OutputText($"You: {textBoxMessage.Text.Trim()}");

            byte[] data = Encoding.UTF8.GetBytes("GET_QUOTE");
            stream.Write(data, 0, data.Length);

            byte[] buf = new byte[1024];
            int bytes = stream.Read(buf, 0, buf.Length);
            string response = Encoding.UTF8.GetString(buf, 0, bytes);
            OutputText($"Server: {response}");
        }

        private void OutputText(string text)
        {
            textBoxChat.AppendText(text + "\r\n");
        }
    }
}
