using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/* ======================================================================
* 
* 作者：Zack 
* 创建时间：2018/3/8 17:02:55
* 文件名：TCPServer
*
* ========================================================================
*/
namespace MaxServer_dll
{
    public class TCPServer
    {
        #region 服务器事件

        /// <summary>
        /// 当收到消息时触发
        /// </summary>
        public Action OnMessageArrived;


        public Action<Socket> OnMessageSend;
        /// <summary>
        /// 当接收到消息后触发
        /// </summary>
        //public Action<Socket,byte[]> OnRecivedMessage;

        /// <summary>
        /// 当有客户端连接时触发
        /// </summary>
        public Action<Socket> OnConnnectServer;

        /// <summary>
        /// 当客户端断开时触发
        /// </summary>
        public Action<Socket> OnDisconnectServer;


        /// <summary>
        /// 当发生错误时调用
        /// </summary>
        public Action<string> Error;
        #endregion



        private Queue<TCPMessage> recvQueue;

        //发送队列
        private Queue<TCPMessage> sendQueue;

        //private IPEndPoint localPoint;
        private TcpListener Server;
        private List<TcpClientInfo> Clients;
        private bool stopListeing = false;
        private bool stopRecieve = false;
        private bool stopSend = false;
        private Thread sendThread;
        public readonly Socket socket;
        public int RecvDataBufferSize = 1024;
        public TCPServer(int LocalPort)
        {
            IPEndPoint localPoint = new IPEndPoint(IPAddress.Any, LocalPort);
            try
            {
                Server = new TcpListener(localPoint);
            }catch(SocketException se)
            {
                Error?.Invoke(se.Message);
                return;
            }
            
            socket = Server.Server;
        }

        public void Start(int MaxConnect)
        {
            Clients = new List<TcpClientInfo>();
            recvQueue = new Queue<TCPMessage>();
            sendQueue = new Queue<TCPMessage>();

            Server.Start(MaxConnect);

            //开启一个线程处理发送
            sendThread = new Thread(new ThreadStart(DoSendMsg))
            {
                IsBackground = true
            };
            sendThread.Start();
            //开启异步监听
            Server.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClient), Server);
            Console.WriteLine("服务器启动");
        }

        public void Send(TCPMessage message)
        {
            sendQueue.Enqueue(message);
        }
        public void SendString(Socket client,string message)
        {
            Console.WriteLine(client);
            Console.WriteLine(message);
            TCPMessage msg = new TCPMessage(socket, client, message);
            Send(msg);
        }

        private void DoSendMsg()
        {
            Console.WriteLine("发送线程启动");

            while (!stopSend)
            {
                if (sendQueue.Count > 0)
                {
                    TCPMessage message = sendQueue.Dequeue();
                    //Console.WriteLine("要发送消息啦" + message.Message);
                    //没有指定发送目标 向所有连接的客户端发送
                    if (message.TargetPoint == null)
                    {
                        foreach(TcpClientInfo tcpinfo in Clients)
                        {
                            tcpinfo.socket.Send(message.data, message.data.Length, SocketFlags.None);
                        }
                        OnMessageSend?.Invoke(null);
                    }
                    else
                    {
                        TcpClientInfo clientInfo = FindTcpClinet(message.TargetPoint);
                        Socket To = null;
                        //通过Socket没有找到
                        if (clientInfo == null)
                        {
                            To = FindClient(message.TargetIP, message.TargetPort);
                            if (To == null)
                            {
                                //TODO:报错，没有找到客户端
                                Error?.Invoke("发送数据时没有找到目标");
                                continue;
                            }
                        }
                        else
                        {
                            To = clientInfo.socket;
                        }

                        To.Send(message.data, message.data.Length, SocketFlags.None);
                        OnMessageSend?.Invoke(To);
                    }
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 根据客户端的Ip和端口查找连接的Socket，没有找到返回空
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        /// <returns>查询的Socket</returns>
        public Socket FindClient(string Ip, int Port)
        {
            Socket socket = null;
            foreach(TcpClientInfo client in Clients)
            {
                if(client.Ip == Ip && client.Port == Port)
                {
                    socket = client.socket;
                    break;
                }
            }
            return socket;
        }


        /// <summary>
        /// 这个服务器类设计为向外暴露Socket，自定义的中间类不要暴露
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        private TcpClientInfo FindTcpClinet(string Ip, int Port)
        {
            TcpClientInfo info = null;
            foreach (TcpClientInfo client in Clients)
            {
                if (client.Ip == Ip && client.Port == Port)
                {
                    info = client;
                    break;
                }
            }

            return info;
        }

        private TcpClientInfo FindTcpClinet(Socket socket)
        {
            TcpClientInfo info = null;
            foreach (TcpClientInfo client in Clients)
            {
                if (client.socket == socket)
                {
                    info = client;
                    break;
                }
            }

            return info;
        }


        /// <summary>
        /// 获取客户端发来的消息，没有则返回空
        /// </summary>
        /// <returns></returns>
        public TCPMessage GetMessage()
        {
            if (recvQueue.Count > 0)
                return recvQueue.Dequeue();
            else
                return null;
        }

        private void DoAcceptTcpClient(IAsyncResult ar)
        {
            if (stopListeing)
                return;

            //还原原始的TcpListner对象
            Server = (TcpListener)ar.AsyncState;
            //完成连接的动作，并返回新的TcpClient  
            TcpClient client = Server.EndAcceptTcpClient(ar);
            TcpClientInfo internalClient = new TcpClientInfo(client, RecvDataBufferSize);
            //Console.WriteLine(internalClient.Ip + ":" + internalClient.Port + "连接成功");
            //更新链接入的客户端数量，将客户端保存至已连接客户端列表中  
            lock (Clients)
            {
                Clients.Add(internalClient);
            }
            OnConnnectServer?.Invoke(client.Client);
            internalClient.socket.BeginReceive(internalClient.RecvBuff, 0, internalClient.RecvBuff.Length, SocketFlags.None, new AsyncCallback(DoReceiveData), internalClient);
            Server.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClient), Server);
        }

        private void DoReceiveData(IAsyncResult ar)
        {
            if (stopRecieve)
                return;

            TcpClientInfo internalClient = (TcpClientInfo)ar.AsyncState;
            NetworkStream networkStream = internalClient.client.GetStream();
            int read = 0;
            try
            {
                read = networkStream.EndRead(ar);
            }
            catch
            {
                read = 0;
            }

            if (read == 0)
            {
                ///客户端已经断开
                lock (Clients)
                {
                    Clients.Remove(internalClient);
                }

                //触发客户端断开事件
                OnDisconnectServer?.Invoke(internalClient.socket);
                try
                {
                    internalClient.socket.Close();
                    internalClient.socket.Dispose();
                    internalClient = null;
                }
                catch(Exception e)
                {

                }
                return;
            }
            byte[] recvData = new byte[read];
            Array.Copy(internalClient.RecvBuff, 0, recvData, 0, read);
            //string ssss = Encoding.UTF8.GetString(recvData);

            //将收到的消息存入队列
            TCPMessage message = new TCPMessage(internalClient.client.Client,this.socket, recvData);
            recvQueue.Enqueue(message);

            //发送消息到达通知
            OnMessageArrived?.Invoke();
            //OnRecivedMessage?.Invoke(internalClient.socket, recvData);
            //Console.WriteLine(internalClient.Ip + ":" + internalClient.Port + "有消息：" + ssss);
            internalClient.socket.BeginReceive(internalClient.RecvBuff, 0, internalClient.RecvBuff.Length, SocketFlags.None, new AsyncCallback(DoReceiveData), internalClient);
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Stop()
        {
            //停止监听
            Server.Stop();

            //停止监听、接收和发送
            stopListeing = true;
            stopRecieve = true;
            stopSend = true;

            //断开所有连接
            CloseAll();
        }



        /// <summary>
        /// 关闭所有的客户端连接
        /// </summary>
        public void CloseAll()
        {
            foreach(TcpClientInfo client in Clients)
            {
                //关闭连接
                if (client.client.Connected)
                    client.socket.Shutdown(SocketShutdown.Both);

                //关闭Socket
                client.socket.Close();

                //关闭tcpClient
                client.client.Close();
            }

            //全部移除
            Clients.RemoveAll(i => true);
        }


        /// <summary>
        /// 关闭指定的客户端
        /// </summary>
        /// <param name="client"></param>
        public void Close(Socket client)
        {
            TcpClientInfo TheClient = null;
            foreach(TcpClientInfo clientInfo in Clients)
            {
                if (client.Equals(clientInfo.socket))
                {
                    TheClient = clientInfo;
                }
            }

            if (TheClient == null)
                return;

            if (TheClient.client.Connected)
                TheClient.socket.Shutdown(SocketShutdown.Both);

            //关闭Socket
            TheClient.socket.Close();

            //关闭tcpClient
            TheClient.client.Close();

            Clients.Remove(TheClient);
            TheClient = null;
        }


        ~TCPServer()
        {
            Stop();
        }
    }

    internal class TcpClientInfo
    {
        public readonly TcpClient client;
        public readonly Socket socket;
        public readonly string Ip;
        public readonly int Port;
        public byte[] RecvBuff;
        public TcpClientInfo(TcpClient client, int MaxBufferSize)
        {
            this.client = client;
            socket = client.Client;
            Ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            RecvBuff = new byte[MaxBufferSize];
        }
    }

}
