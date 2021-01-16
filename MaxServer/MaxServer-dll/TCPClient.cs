using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/* ======================================================================
* 
* 作者：Zack 
* 创建时间：2018/3/3 13:32:56
* 文件名：TCPClient
*
* ========================================================================
*/
namespace MaxServer_dll
{

    /// <summary>
    /// 一个封装好的tcp客户端类，支持断开后自动重连
    /// </summary>
    public class TCPClient
    {
        private IPAddress host ;
        private int Port = -1;

        private IPEndPoint localPoint = null;

        private TcpClient client;
        private bool __connected = false;
        #region 客户端事件
        /// <summary>
        /// 当收到消息时触发
        /// </summary>
        public Action OnMessageArrived;

        /// <summary>
        /// 当消息发送后触发
        /// </summary>
        public Action OnMessageSend;

        /// <summary>
        /// 连接成功时触发
        /// </summary>
        public Action OnClinetConnnect;

        /// <summary>
        /// 连接断开时触发
        /// </summary>
        public Action OnClinetDisconnect;

        #endregion



        public bool connected
        {
            get { return __connected; }
        }
        private NetworkStream stream;

        private  Socket _socket;

        public Socket socket { get { return _socket; } }

        private Thread recvProcess = null, sendProcess = null;

        private volatile bool stopSendProcess, stopRecvProcess;


        private int maxBufferSize = 1024;

        public int MaxBufferSize
        {
            set
            {
                maxBufferSize = value;
            }
            get
            {
                return maxBufferSize;
            }
        }
        private Queue<byte[]> recvQueue;

        //发送队列
        private Queue<byte[]> sendQueue;

        /// <summary>
        /// 新建一个指定端口的客户端
        /// </summary>
        /// <param name="localPort">指定的端口</param>
        public TCPClient(int localPort):this()
        {
            localPoint = new IPEndPoint(IPAddress.Any, localPort);
        }


       /// <summary>
       /// 新建一个客户端
       /// </summary>
        public TCPClient()
        {
            sendQueue = new Queue<byte[]>();
            recvQueue = new Queue<byte[]>();
        }

        /// <summary>
        /// 开启客户端并连接服务器
        /// </summary>
        /// <param name="ServerIp">服务器ip</param>
        /// <param name="ServerPort">服务器端口</param>
        /// <param name="isRecevie">是否同时接收来自服务器的消息</param>
        public void Start(string ServerIp, int ServerPort, bool isRecevie)
        {

            if(!IPAddress.TryParse(ServerIp,out host))
            {
                throw new Exception("IP不正确");
            }
            Port = ServerPort;
            stopSendProcess = false;
            stopRecvProcess = !isRecevie;
            sendProcess = new Thread(new ThreadStart(SendProcess))
            {
                IsBackground = true
            };
            sendProcess.Start();

        }


        /// <summary>
        /// 获取网络传递过来的数据，没有返回null
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetMessage()
        {
            if (!stopRecvProcess && recvQueue.Count > 0)
                return recvQueue.Dequeue();
            return null;
        }

        public string GetMessageString()
        {
            byte[] data = GetMessage();
            if (data == null)
                return string.Empty;
            return Encoding.UTF8.GetString(data);
        }

        private void TryConnect()
        {
            CreateConnection(host, Port);
        }

        void CreateConnection(IPAddress host, int port)
        {
            try
            {
                if (localPoint == null)
                    client = new TcpClient();
                else
                    client = new TcpClient(localPoint);

                client.NoDelay = true;

                ///这里采用异步连接
                IAsyncResult result = client.BeginConnect(host, port, null, null);
                __connected = result.AsyncWaitHandle.WaitOne(1000, false);

                if (__connected)
                {
                    client.EndConnect(result);
                    _socket = client.Client;
                }
                else
                {
                    client.Close();
                    client = null;
                }
            }
            catch (SocketException ex)
            {
                __connected = false;
                client.Close();
                client = null;
                return;
            }

            if (__connected)
            {
                Console.WriteLine("连接成功!");
                stream = client.GetStream();
                if (!stopRecvProcess && recvProcess == null)
                {
                    recvProcess = new Thread(new ThreadStart(RecvProcess))
                    {
                        IsBackground = true
                    };
                    recvProcess.Start();
                }
                OnClinetConnnect?.Invoke();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Data"></param>
        public void Send(Byte[] Data)
        {
            sendQueue.Enqueue(Data);
        }
        public void Send(string Message)
        {
            byte[] data = Encoding.UTF8.GetBytes(Message);
            Send(data);
        }

        //数据发送线程，
        void SendProcess()
        {
            Console.WriteLine("发送线程");
            while (!stopSendProcess)
            {
                if (!__connected)
                {
                    Console.WriteLine("连接中...");
                    TryConnect();
                }
                while (sendQueue.Count > 0 && __connected)
                {
                    Byte[] buffer = sendQueue.Dequeue();
                    Console.WriteLine("发送数据---");
                    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(item.Serialize());
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();

                    ///触发事件
                    OnMessageSend?.Invoke();
                }
                Thread.Sleep(10);
            }
        }

        private void RecvProcess()
        {
            
            while (!stopRecvProcess && stream.CanRead)
            {
                byte[] recvBuf = new byte[maxBufferSize];
                int bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(recvBuf, 0, maxBufferSize);
                    
                }
                catch
                {

                }

                Console.WriteLine("这里会走么" + bytesRead);
                if (bytesRead < 1)
                    break;

                byte[] recvData = new byte[bytesRead];

                //只保留数组有数据的部分,空的部分没必要占内存
                Array.Copy(recvBuf, 0, recvData, 0, bytesRead);
                recvQueue.Enqueue(recvData);
                //Console.WriteLine(Encoding.UTF8.GetString(recvData));
                OnMessageArrived?.Invoke();
            }
            __connected = false;
            recvProcess = null;
            OnClinetDisconnect?.Invoke();

            stream.Close();
            stream.Dispose();
            client.Close();
            client = null;
            Console.WriteLine("断开连接");
        }

        ~TCPClient()
        {
            Stop();

        }

        /// <summary>
        /// 关闭客户端连接
        /// </summary>
        public void Stop()
        {
            stopSendProcess = true;
            stopRecvProcess = true;

            if (__connected)
            {
                __connected = false;

                stream.Close();
                stream.Dispose();
                client.Close();
                client = null;
            }

            if (recvProcess != null)
            {
                //recvProcess.Abort();
                // 如果没有正确关闭线程，这里的Join就会阻塞，就会卡死编辑器
                // recvProcess.Join();
                //Debug.Log("recvProcess: " + recvProcess.IsAlive);
            }

            if (sendProcess != null)
            {
                //sendProcess.Abort();

                // sendProcess.Join();
                //Debug.Log("sendProcess: " + sendProcess.IsAlive);
            }
        }

    }
}
