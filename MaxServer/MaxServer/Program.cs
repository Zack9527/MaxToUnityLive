using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MaxServer_dll;

namespace MaxServer
{
    class Program
    {
        private static TCPServer server;
        private static TCPClient client;
        static void Main(string[] args)
        {
            //Socket client = null;
            //server = new TCPServer(3698);
            //server.OnMessageArrived = ()=> {
            //    string msg = server.GetMessage().Message;
            //    Console.WriteLine("收到消息:" + msg);
            //};
            //server.OnConnnectServer += (Socket so) =>
            //{
            //    client = so;
            //    Console.WriteLine(so.ToString() + "连接成功");
            //};
            //server.OnDisconnectServer += (Socket so) =>
            //{
            //    Console.WriteLine(so.ToString() + "断开");
            //};
            //server.Start (10);
            client = new TCPClient(3654);
            client.OnClinetConnnect = () =>
            {
                Console.WriteLine("客户端连接上了");
            };
            client.OnMessageArrived = () =>
            {
                string msg = client.GetMessageString();
                Console.WriteLine("收到数据" + msg);
            };
            client.Start("127.0.0.1", 3698, true);




            while (true)
            {
                string msg = Console.ReadLine();
                if(msg == "close")
                {
                    client.Stop();
                }
                else
                {
                    client.Send(msg);
                }
                
            }
        }
    }
}
