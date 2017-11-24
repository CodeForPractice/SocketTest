using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：RemotingClient.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 14:04:47
    /// </summary>
    public class RemotingClient
    {
        private static readonly byte[] TimeoutMessage = Encoding.UTF8.GetBytes("Remoting request timeout.");
        private readonly SocketSetting _setting;

        private IPEndPoint _serverEndPoint;
        private ClientSocket _clientSocket;
        private bool _started = false;

        public bool IsConnected
        {
            get { return _clientSocket != null && _clientSocket.IsConnected; }
        }
        public EndPoint ServerEndPoint
        {
            get { return _serverEndPoint; }
        }
        public ClientSocket ClientSocket
        {
            get { return _clientSocket; }
        }

        public RemotingClient(IPEndPoint serverEndPoint, SocketSetting setting = null)
        {
            _serverEndPoint = serverEndPoint;
            _setting = setting ?? new SocketSetting();
        }
    }
}
