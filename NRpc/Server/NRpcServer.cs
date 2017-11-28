using NRpc.Transport.Remoting;
using NRpc.Transport.Socketing;
using System.Net;

namespace NRpc.Server
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：NRpcServer.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：NRpcServer
    /// 创建标识：yjq 2017/11/27 21:12:35
    /// </summary>
    public class NRpcServer
    {
        private IPEndPoint _iPEndPoint;
        private readonly SocketRemotingServer _socketRemotingServer;

        public NRpcServer(int port)
        {
            _iPEndPoint = new IPEndPoint(SocketUtils.GetLocalIPV4(), port);
            _socketRemotingServer = new SocketRemotingServer(_iPEndPoint).RegisterRequestHandler(100, new NRpcHandle());
        }

        public void Start()
        {
            _socketRemotingServer.Start();
        }

        public void ShutDown()
        {
            _socketRemotingServer?.Shutdown();
        }
    }
}