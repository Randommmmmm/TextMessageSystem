using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 短信收发系统
{
    class Client
    {
        public string name = "1022";
        public Socket socketClient;
        private static IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.105"), 8080);
        

        public Client()
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint clientPort = new IPEndPoint(IPAddress.Parse("192.168.1.109"), Int32.Parse(name));

            try
            {
                socketClient.Bind(clientPort);
                socketClient.Connect(ipEndPoint);
                Console.WriteLine("连接上服务器：" + socketClient.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //~Client()
        //{
        //    socketClient.Close();
        //}

    }
}
