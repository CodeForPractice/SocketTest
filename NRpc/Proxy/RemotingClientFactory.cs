using NRpc.Transport.Remoting;
using NRpc.Transport.Socketing;
using NRpc.Utils;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace NRpc.Proxy
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：RemotingClientFactory.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/27 16:28:27
    /// </summary>
    internal class RemotingClientFactory
    {
        private static ConcurrentDictionary<EndPoint, SocketRemotingClient> WaitConnectingClientList = new ConcurrentDictionary<EndPoint, SocketRemotingClient>();
        private static ConcurrentDictionary<EndPoint, SocketRemotingClient> ConnectedClientList = new ConcurrentDictionary<EndPoint, SocketRemotingClient>();

        public static SocketRemotingClient GetClient(Type classType)
        {
            var className = classType.FullName;
            var ipEndPoint = new IPEndPoint(SocketUtils.GetLocalIPV4(), 12345);
            if (ConnectedClientList.ContainsKey(ipEndPoint))
            {
                return ConnectedClientList[ipEndPoint];
            }
            SocketRemotingClient client = new SocketRemotingClient(new IPEndPoint(SocketUtils.GetLocalIPV4(), 12345));
            WaitConnectingClientList.TryAdd(client.ServerEndPoint, client);
            client.Start();
            if (ConnectedClientList[ipEndPoint] == null)
            {
                throw new ArgumentNullException("Client", "连接远程失败");
            }
            return client;
        }

        private class ProxyClientConnectionLister : IConnectionEventListener
        {
            public void OnConnectionAccepted(ITcpConnection connection)
            {
            }

            public void OnConnectionClosed(ITcpConnection connection, SocketError socketError)
            {
                ConnectedClientList.TryRemove(connection.RemotingEndPoint, out SocketRemotingClient client);
            }

            public void OnConnectionEstablished(ITcpConnection connection)
            {
                if (WaitConnectingClientList.TryRemove(connection.RemotingEndPoint, out SocketRemotingClient client))
                {
                    ConnectedClientList.TryAdd(connection.RemotingEndPoint, client);
                }
            }

            public void OnConnectionFailed(EndPoint remotingEndPoint, SocketError socketError)
            {
                WaitConnectingClientList.TryRemove(remotingEndPoint, out SocketRemotingClient client);
            }
        }

        public static void RegisterUnLoad()
        {
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            LogUtil.Debug("执行关闭客户端方法");
            foreach (var item in WaitConnectingClientList)
            {
                item.Value?.Shutdown();
            }
            foreach (var item in ConnectedClientList)
            {
                item.Value?.Shutdown();
            }
        }
    }
}