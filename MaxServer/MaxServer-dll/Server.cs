using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MaxServer_dll
{
    public class Server
    {
        public int ServerPort = 7896;
        private TCPServer server;
        public List<LiveClientSide> clients;
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

        public LiveClientSide GetClientByPort(string ip,int port)
        {
            LiveClientSide client = null;
            foreach(LiveClientSide ls in clients)
            {
                if (ls.IP == ip && ls.Port == port)
                {
                    client = ls;
                    break;
                }
                   
            }
            return client;
        }

        public LiveClientSide GetClientBySocket(Socket socket)
        {
            LiveClientSide client = null;
            foreach (LiveClientSide ls in clients)
            {
                if (ls.Client == socket)
                {
                    client = ls;
                    break;
                }
            }
            return client;
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
            string msgStr = tcpMsg.Message;
            Debug.Log(msgStr);
            try
            {
                Dictionary<string, string> form = JsonHelper.Deserialize<Dictionary<string, string>>(msgStr);
                if(form == null || form.Count == 0){
                    Debug.Log("收到无效消息");
                }
                MessageHandler handler = HandlerFactory.Produce(form, tcpMsg.SourcePoint, this);
                handler.Handle();
            }
            catch (Exception e)
            {
                Debug.Error(e.Message);
            }
        }

        public void Send(string msg,Socket client)
        {
            server.Send(client, msg);
        }
        public void Send(string msg,string clientID)
        {
            LiveClientSide Client = GetClientByID(clientID);
            if(Client == null)
            {
                Debug.Error(string.Format("指定的客户端ID不存在:{0}", clientID));
                return;
            }
            Send(msg, Client.Client);
        }

        public void SendToLabel(string msg,string label)
        {
            foreach(LiveClientSide client in clients)
            {
                if (client.Label == label)
                    Send(msg, client.Client);
            }
        }

        private void onClinetDisConnent(Socket client)
        {
            RemoveClient(client);
            Console.WriteLine("客户端断开连接,现在连接数:" + server.GetClients().Length);
        }

        private void onClinetConnent(Socket client)
        {
            Console.WriteLine("客户端建立连接,现在连接数:" + server.GetClients().Length);
        }
    }
}
