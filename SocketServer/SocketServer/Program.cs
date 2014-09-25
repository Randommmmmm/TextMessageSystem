using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int sendPicPort = 600;//发送图片的端口
            int recvCmdPort = 400;//接收请求的端口开启后就一直进行侦听


            SocketServer socketServer = new SocketServer();

            Thread tSocketServer = new Thread(new ThreadStart(socketServerProcess.thread));//线程开始的时候要调用的方法为threadProc.thread
            tSocketServer.IsBackground = true;//设置IsBackground=true,后台线程会自动根据主线程的销毁而销毁 　
            tSocketServer.Start();
            Console.ReadKey();
        }
    }
}
