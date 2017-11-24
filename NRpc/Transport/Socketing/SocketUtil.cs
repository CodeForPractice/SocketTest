using NRpc.Utils;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NRpc.Transport.Socketing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SocketUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 17:34:33
    /// </summary>
    public class SocketUtils
    {
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
        }

        public static Socket CreateSocket(int sendBufferSize, int receiveBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Blocking = false;
            socket.SendBufferSize = sendBufferSize;
            socket.ReceiveBufferSize = receiveBufferSize;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            return socket;
        }

        public static void ShutdownSocket(Socket socket)
        {
            if (socket == null) return;

            ExceptionUtil.Eat(() => socket.Shutdown(SocketShutdown.Both));
            ExceptionUtil.Eat(() => socket.Close(10000));
        }

        public static void CloseSocket(Socket socket)
        {
            if (socket == null) return;

            ExceptionUtil.Eat(() => socket.Close(10000));
        }
    }
}