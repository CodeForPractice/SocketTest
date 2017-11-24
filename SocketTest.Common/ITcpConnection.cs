using System.Net;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ITcpConnection.cs
    /// 接口属性：公共
    /// 类功能描述：ITcpConnection接口
    /// 创建标识：yjq 2017/11/24 15:04:53
    /// </summary>
    public interface ITcpConnection
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 本地连接地址
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// 远程连接地址
        /// </summary>
        EndPoint RemotingEndPoint { get; }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(byte[] message);

        /// <summary>
        /// 关闭当前连接
        /// </summary>
        void Close();
    }
}