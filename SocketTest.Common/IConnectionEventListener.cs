using System.Net.Sockets;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：IConnectionEventListener.cs
    /// 接口属性：公共
    /// 类功能描述：IConnectionEventListener接口
    /// 创建标识：yjq 2017/11/24 15:03:10
    /// </summary>
    public interface IConnectionEventListener
    {
        /// <summary>
        /// 接收到连接
        /// </summary>
        /// <param name="connection"></param>
        void OnConnectionAccepted(ITcpConnection connection);

        /// <summary>
        /// 连接建立
        /// </summary>
        /// <param name="connection"></param>
        void OnConnectionEstablished(ITcpConnection connection);

        /// <summary>
        /// 连接失败
        /// </summary>
        /// <param name="socketError"></param>
        void OnConnectionFailed(SocketError socketError);

        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="socketError"></param>
        void OnConnectionClosed(ITcpConnection connection, SocketError socketError);
    }
}