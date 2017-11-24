using System.Net;

namespace NRpc.Transport.Socketing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ITcpConnection.cs
    /// 接口属性：公共
    /// 类功能描述：ITcpConnection接口
    /// 创建标识：yjq 2017/11/24 16:22:45
    /// </summary>
    public interface ITcpConnection
    {
        bool IsConnected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemotingEndPoint { get; }

        void SendMessage(byte[] message);

        void Close();
    }
}