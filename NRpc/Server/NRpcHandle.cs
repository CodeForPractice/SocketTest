using NRpc.Transport.Remoting;

namespace NRpc.Server
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：NRpcHandle.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：NRpcHandle
    /// 创建标识：yjq 2017/11/27 21:10:37
    /// </summary>
    internal sealed class NRpcHandle : IRequestHandler
    {
        private readonly ServerMethodCaller _serverMethodCaller;

        public NRpcHandle()
        {
            _serverMethodCaller = new ServerMethodCaller();
        }

        public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest)
        {
            return _serverMethodCaller.HandleRequest(remotingRequest);
        }
    }
}