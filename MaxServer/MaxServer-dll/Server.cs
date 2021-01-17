using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MaxServer_dll
{
    public class Server
    {
        public int ServerPort = 7896;
        private TCPServer server;
        private List<LiveClientSide> clients;
        public void Start()
        {
            server = new TCPServer(ServerPort);
            server.OnConnnectServer += onClinetConnent;
            server.OnDisconnectServer += onClinetDisConnent;
            server.OnMessageArrived += onMessage;
            clients = new List<LiveClientSide>();
            server.Start(10);
        }

        public void AddClient(LiveClientSide client)
        {
            if (clients.Contains(client))
                return;
            clients.Add(client);
        }
        public void RemoveClient(LiveClientSide client)
        {
            if (clients.Contains(client))
                clients.Remove(client);
        }
        public void RemoveClient(Socket client)
        {
            for(int i = 0; i < clients.Count; i++)
            {
                if(clients[i].Client == client)
                {
                    clients.RemoveAt(i);
                    break;
                }

            }
        }

        public LiveClientSide GetClientByID(string id)
        {
            foreach(LiveClientSide client in clients)
            {
                if (client.ID == id)
                    return client;
            }
            return null;
        }
        public void Stop()
        {
            server.OnConnnectServer -= onClinetConnent;
            server.OnDisconnectServer -= onClinetDisConnent;
            server.OnMessageArrived -= onMessage;
            server.CloseAll();
            server.Stop();
            server = null;
        }

        private void onMessage()
        {
            TCPMessage tcpMsg = server.GetMessage();
            if (tcpMsg == null)
                return;
            Console.WriteLine(string.Format("{0}:{1}说:{2}", tcpMsg.SrcIP, tcpMsg.SrcPort, tcpMsg.Message));
        }

        private void onClinetDisConnent(Socket client)
        {
            Console.WriteLine("客户端建立连接,现在连接数:" + server.GetClients().Length);
        }

        private void onClinetConnent(Socket client)
        {
            Console.WriteLine("客户端断开连接,现在连接数:" + server.GetClients().Length);
        }
    }
}
