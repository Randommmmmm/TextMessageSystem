using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
namespace SocketServer
{
    class SocketServer
    {
        private Socket socketServer;
        private static IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.102"), 8080);
        private static int CountClient = 0;

        struct sumClients//标记所有用户连接状态
        {
            public Socket socket;
            public bool online;
        }
        sumClients[] onlineClients = new sumClients[1000000];


        struct DelayMSG//记录对应客户端的DelayMSG
        {
            public String D_msg;
        }
        struct DelayDATA
        {
            public DelayMSG[] delayMsg;
            public int sum;
            public bool flag;
        }
        DelayDATA[] DelayPort = new DelayDATA[100000];

        public SocketServer()
        {
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(ipEndPoint);
            socketServer.Listen(1000);
            Console.WriteLine("启动服务");

            while (true)
            {
                Socket client = null;
                //Console.WriteLine(currentTime.Hour + ":" + currentTime.Minute + ":" + currentTime.Second);
                try
                {
                    client = socketServer.Accept();
                    CountClient++;
                    Console.WriteLine("客户端" + CountClient + "连接" + client.RemoteEndPoint.ToString());
                    //记录所有在线用户
                    int clientPorts = Int32.Parse(client.RemoteEndPoint.ToString().Substring(14, 4));
                    onlineClients[clientPorts].online = true;
                    onlineClients[clientPorts].socket = client;

                    if (DelayPort[clientPorts].flag == false)//第一次登录客户端时进行DelayMSG初始化
                    {
                        DelayPort[clientPorts].delayMsg = new DelayMSG[100000];
                        DelayPort[clientPorts].sum = 0;
                        DelayPort[clientPorts].flag = true;
                    }

                    int i = 0;
                    while (i < DelayPort[clientPorts].sum)
                    {
                        SendDelayData(DelayPort[clientPorts].delayMsg[i].D_msg, client, client.RemoteEndPoint.ToString().Substring(14, 4));

                        int fromPort = Int32.Parse(DelayPort[clientPorts].delayMsg[i].D_msg.Substring(1, 4));
                        if (fromPort != 8080)
                        {
                            string msg = "成功发送给用户" + clientPorts.ToString();
                            
                            if (onlineClients[fromPort].online == true)//回执
                            {
                                SendDataFromInput(onlineClients[fromPort].socket, msg, "8080", fromPort.ToString());
                            }
                            else
                            {
                                int j = DelayPort[fromPort].sum;
                                DelayPort[fromPort].delayMsg[j].D_msg = ":8080" + "@" + fromPort.ToString() + msg;
                                DelayPort[fromPort].sum++;
                            }
                        }
                        DelayPort[clientPorts].delayMsg[i].D_msg = "";
                        i++;
                    }
                    DelayPort[clientPorts].sum = 0;

                    new Thread(ReceData).Start(client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "1");
                }
            }
        }
        //延时发送
        private void SendDelayData(String msg, Socket client, string to)
        {
            Byte[] sendData;
            try
            {
                sendData = new Byte[8192];

                sendData = Encoding.Unicode.GetBytes(msg);
                client.Send(sendData);
                Console.WriteLine("向客户端" + to + "发送：" + msg);
            }
            catch (Exception e)
            {
                Console.WriteLine("SendDelayData:" + e.Message);
            }
        }


        private void SendDataFromInput(Object client, string msg, string from, string  to)
        {
            Socket NowClient = client as Socket;

            msg = ":" + from + "@" + to + msg;

            try
            {
                Byte[] sendData;
                sendData = new Byte[8192];

                sendData = Encoding.Unicode.GetBytes(msg);
                NowClient.Send(sendData);
                Console.WriteLine("向客户端" + to + "发送：" + msg);
            }
            catch (Exception e) 
            {
                return;
            }
        }


        String msgs = null;
        //Socket sendMsgClient;

        private void ReceData(Object client)
        {
            Socket socketClient = (Socket)client;
            Byte[] receData;
            int receDataLen;
            while (true)
            {
                try
                {
                    receData = new Byte[8192];
                    receDataLen = socketClient.Available;
                    socketClient.Receive(receData, receDataLen, SocketFlags.None);//等待接收

                    if (receDataLen == 0)
                        continue;

                    msgs = System.Text.Encoding.Unicode.GetString(receData, 0, receDataLen);
                    Console.WriteLine("接受到客户端：" + msgs);
                    char[] sep = { ':', '@' };
                    string[] info = msgs.Split(sep);

                    int i;
                    for (i = 2; i < info.Length - 1; i++)
                    {
                        if (onlineClients[Int32.Parse(info[i])].socket == null)
                        {
                            string msg = "用户" + info[i] + "是空号";
                            SendDataFromInput(onlineClients[Int32.Parse(info[1])].socket, msg, "8080", info[1]);
                            break;
                        }
                    }

                    if (i != info.Length - 1)
                        continue;

                    if (info[0] == "")
                    {
                        for (i = 2; i < info.Length - 1; i++)
                        {
                            if (onlineClients[Int32.Parse(info[i])].online == true)
                            {
                                SendDataFromInput(onlineClients[Int32.Parse(info[i])].socket, info[info.Length - 1], info[1], info[i]);

                                string msg = "成功发送给用户" + info[i];
                                SendDataFromInput(onlineClients[Int32.Parse(info[1])].socket, msg, "8080", info[1]);
                            }
                            else
                            {
                                int j = DelayPort[Int32.Parse(info[i])].sum;
                                DelayPort[Int32.Parse(info[i])].delayMsg[j].D_msg = ":" + info[1] + "@" + info[i] + info[info.Length - 1];
                                DelayPort[Int32.Parse(info[i])].sum++;

                                string msg = "用户" + info[i] + "不在线，发送暂缓";
                                SendDataFromInput(onlineClients[Int32.Parse(info[1])].socket, msg, "8080", info[1]);
                            }
                        }
                    }
                    else
                    {
                        string curTime = DateTime.Now.ToString();

                        char[] sepT = { ' ', ':' };
                        string[] curT = curTime.Split(sepT);

                        int curHour = Int32.Parse(curT[1]);
                        int curMin = Int32.Parse(curT[2]);
                        int curSec = Int32.Parse(curT[3]);

                        string setTime = info[0].Substring(1, 6);
                        int setHour = Int32.Parse(setTime.Substring(0, 2));
                        int setMin = Int32.Parse(setTime.Substring(2, 2));
                        int setSec = Int32.Parse(setTime.Substring(4, 2));

                        for (i = 2; i < info.Length - 1; i++)
                        {
                            int time = (setHour - curHour) * 3600 + (setMin - curMin) * 60 + setSec - curSec;
                            string msg = ":" + info[1] + "@" + info[i] + " " + info[info.Length - 1] + " " + time.ToString();

                            new Thread(Counter).Start(msg);
                        }
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine("ReceData:" + e.Message);
                    //下线用户
                    int cutline_ports = Int32.Parse(socketClient.RemoteEndPoint.ToString().Substring(14, 4));
                    onlineClients[cutline_ports].online = false;
                    onlineClients[cutline_ports].socket.Close();
                    break;
                }
            }   
        }

        private void Counter(Object m)
        {
            string info = (string)m;
            string[] str = info.Split(' ');
            int time = Int32.Parse(str[2]);
            string msg = str[1];
            while (time > 0)
            {
                time--;
                Thread.Sleep(1000);
            }
            int fromPort = Int32.Parse(str[0].Substring(1, 4));
            int toPort = Int32.Parse(str[0].Substring(6, 4));
            if (onlineClients[toPort].online == true)
            {
                SendDataFromInput(onlineClients[toPort].socket, msg, fromPort.ToString(), toPort.ToString());

                msg = "成功发送给用户" + toPort;
                SendDataFromInput(onlineClients[fromPort].socket, msg, "8080", fromPort.ToString());
            }
            else
            {
                int i = DelayPort[toPort].sum;
                DelayPort[toPort].delayMsg[i].D_msg = ":" + fromPort.ToString() + "@" + toPort.ToString() + msg;
                DelayPort[toPort].sum++;

                msg = "用户" + toPort + "不在线，发送暂缓";
                SendDataFromInput(onlineClients[fromPort].socket, msg, "8080", fromPort.ToString());
            }
        }
    }
}
