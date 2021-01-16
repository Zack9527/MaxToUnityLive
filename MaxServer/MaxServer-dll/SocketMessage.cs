
using System.Net.Sockets;
using System.Net;
using System;

/* ======================================================================
* 
* 作者：Zack 
* 创建时间：2018/3/10 11:59:13
* 文件名：SocketMessage
*
* ========================================================================
*/
namespace MaxServer_dll
{
    public class TCPMessage
    {
        public readonly Socket SourcePoint;
        public readonly Socket TargetPoint;
        public readonly byte[] data;

        /// <summary>
        /// 获取传送者的IP
        /// </summary>
        public string SrcIP { 
            get
            {
                return GetIP(SourcePoint);
            } 
        }

        /// <summary>
        /// 获取传送者的端口
        /// </summary>

        public int SrcPort
        {
            get
            {
                return GetPort(SourcePoint);
            }
        }

        /// <summary>
        /// 获取传送者的IP
        /// </summary>
        public string TargetIP
        {
            get
            {
                return GetIP(TargetPoint);
            }
        }

        /// <summary>
        /// 获取传送者的端口
        /// </summary>

        public int TargetPort
        {
            get
            {
                return GetPort(TargetPoint);
            }
        }

        private string GetIP(Socket socket)
        {
            string ip = string.Empty;
            if (socket == null)
                return ip;
            try
            {
                ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            }
            catch (Exception e)
            {
                try
                {
                    ip = ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
                }
                catch (Exception e2) { }

            }
            return ip;
        }

        private int GetPort(Socket socket)
        {
            int port = -1;
            if (socket == null)
                return port;
            try
            {
                port = ((IPEndPoint)socket.RemoteEndPoint).Port;
            }
            catch (Exception e)
            {
                try
                {
                    port = ((IPEndPoint)socket.LocalEndPoint).Port;
                }
                catch (Exception e2) { }

            }
            return port;
        }

        /// <summary>
        /// 获取传送的信息
        /// </summary>
        public string Message
        {
            get
            {
                return System.Text.Encoding.UTF8.GetString(data);
            }
        }

        public TCPMessage(Socket source,Socket target,byte[] data)
        {
            SourcePoint = source;
            TargetPoint = target;
            this.data = data;
        }
        public TCPMessage(Socket source, Socket target, string message)
        {
            SourcePoint = source;
            TargetPoint = target;
            data = System.Text.Encoding.UTF8.GetBytes(message);

        }
    }

}
