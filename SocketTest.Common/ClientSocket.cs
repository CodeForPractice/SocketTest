using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ClientSocket.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/22 11:46:13
    /// </summary>
    public class ClientSocket
    {
        private Socket _socket;
        private ManualResetEvent _manualResetEvent;
        private TcpConnetion _tcpConnetion;

        public ClientSocket()
        {
            _socket = SocketUtil.Create(10 * 1024, 10 * 1024);
            _manualResetEvent = new ManualResetEvent(false);
        }

        public ClientSocket Start(string ip, int port)
        {
            var connectArgs = new SocketAsyncEventArgs();
            connectArgs.AcceptSocket = _socket;
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            connectArgs.RemoteEndPoint = ipEndPoint;
            connectArgs.Completed += ConnectArgs_Completed;
            var willRaiseEvent = _socket.ConnectAsync(connectArgs);
            if (!willRaiseEvent)
            {
                ProcessConnect(connectArgs);
            }
            _manualResetEvent.WaitOne();
            return this;
        }

        private void ConnectArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                LogUtil.Warn("连接服务端失败");
                _socket.ShutDownCurrent();
                _manualResetEvent.Set();
                return;
            }
            LogUtil.Info("连接服务端成功");
            _tcpConnetion = new TcpConnetion(_socket, MessageArrived);
            _manualResetEvent.Set();
        }


        public void SendMessage(byte[] messageBytes)
        {
            _tcpConnetion.SendMessage(messageBytes);
        }

        private void MessageArrived(TcpConnetion tcpConnetion, byte[] messageBytes)
        {
            LogUtil.Info($"接收来自客户端发送的信息:{tcpConnetion.RemotingEndPoin}:【{Encoding.UTF8.GetString(messageBytes)}】");
        }
    }
}
