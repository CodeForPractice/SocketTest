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
        private AutoResetEvent _autoResetEvent;
        private TcpConnetion _tcpConnetion;
        private IPEndPoint _hostIPEndPoint;
        private Action<TcpConnetion, byte[]> _messageArrivedHandler;
        private SocketSetting _socketSetting;

        public ClientSocket(IPEndPoint iPEndPoint, SocketSetting socketSetting, Action<TcpConnetion, byte[]> messageArrivedHandler)
        {
            _socket = SocketUtil.Create(socketSetting.ReceiveBufferSize, socketSetting.SendBufferSize);
            _autoResetEvent = new AutoResetEvent(false);
            _hostIPEndPoint = iPEndPoint;
            _messageArrivedHandler = messageArrivedHandler;
            _socketSetting = socketSetting;
        }

        public bool IsConnected
        {
            get { return _tcpConnetion != null && _tcpConnetion.IsConnected; }
        }
        public TcpConnetion Connection
        {
            get { return _tcpConnetion; }
        }

        public ClientSocket Start()
        {
            var connectArgs = new SocketAsyncEventArgs();
            connectArgs.AcceptSocket = _socket;
            connectArgs.RemoteEndPoint = _hostIPEndPoint;
            connectArgs.Completed += ConnectArgs_Completed;
            var willRaiseEvent = _socket.ConnectAsync(connectArgs);
            if (!willRaiseEvent)
            {
                ProcessConnect(connectArgs);
            }
            _autoResetEvent.WaitOne();
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
                _autoResetEvent.Set();
                return;
            }
            LogUtil.Info("连接服务端成功");
            _tcpConnetion = new TcpConnetion(_socket, MessageArrived, _socketSetting);
            _autoResetEvent.Set();
        }


        public void SendMessage(byte[] messageBytes)
        {
            _tcpConnetion.SendMessage(messageBytes);
        }

        private void MessageArrived(TcpConnetion tcpConnetion, byte[] messageBytes)
        {
            try
            {
                _messageArrivedHandler?.Invoke(tcpConnetion, messageBytes);
                LogUtil.Info($"接收来自客户端发送的信息:{tcpConnetion.RemotingEndPoint}:【{Encoding.UTF8.GetString(messageBytes)}】");
            }
            catch (Exception e)
            {
                LogUtil.Error(e);
            }
        }
    }
}
