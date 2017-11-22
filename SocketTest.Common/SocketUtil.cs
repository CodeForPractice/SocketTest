using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SocketUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：SocketUtil
    /// 创建标识：yjq 2017/11/21 22:33:24
    /// </summary>
    public static class SocketUtil
    {
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(m => m.AddressFamily == AddressFamily.InterNetwork);
        }

        /// <summary>
        /// 创建socket
        /// </summary>
        /// <param name="receiveBufferSize">接收的buffer长度</param>
        /// <param name="sendBufferSize">发送的buffer长度</param>
        /// <returns>socket</returns>
        public static Socket Create(int receiveBufferSize, int sendBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendBufferSize = sendBufferSize,
                ReceiveBufferSize = receiveBufferSize,
                NoDelay = true,
                Blocking = false
            };
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            return socket;
        }

        public static void ShutDownCurrent(this Socket socket)
        {
            if (socket != null)
            {
                ExceptionUtil.Eat(() => socket.Shutdown(SocketShutdown.Both));
                socket.CloseCurrent();
            }
        }

        public static void CloseCurrent(this Socket socket)
        {
            if (socket != null)
            {
                ExceptionUtil.Eat(() => socket.Close(10000));
            }
        }

        public static void DisposeCurrent(this SocketAsyncEventArgs e, EventHandler<SocketAsyncEventArgs> Completed)
        {
            if (e != null)
            {
                ExceptionUtil.Eat(() =>
                {
                    e.Completed -= Completed;
                    e.AcceptSocket = null;
                    e.Dispose();
                });
            }
        }
    }
}