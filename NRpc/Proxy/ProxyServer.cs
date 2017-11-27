using NRpc.Container;
using NRpc.Serializing;
using NRpc.Transport.Remoting;
using NRpc.Transport.Socketing;
using NRpc.Utils;
using System;
using System.Net;

namespace NRpc.Proxy
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ProxyServer.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/27 17:31:20
    /// </summary>
    public class ProxyServer
    {
        private IPEndPoint _iPEndPoint;
        private IBinarySerializer _binarySerializer;
        private readonly SocketRemotingServer _socketRemotingServer;

        public ProxyServer(int port)
        {
            _iPEndPoint = new IPEndPoint(SocketUtils.GetLocalIPV4(), port);
            _socketRemotingServer = new SocketRemotingServer(_iPEndPoint).RegisterRequestHandler(100, new RequestHandler());
            _binarySerializer = ContainerManager.Resolve<IBinarySerializer>();
        }

        public void Start()
        {
            _socketRemotingServer.Start();
        }

        public void ShutDown()
        {
            _socketRemotingServer?.Shutdown();
        }

        private class RequestHandler : IRequestHandler
        {
            private readonly byte[] response = new byte[0];

            public RequestHandler()
            {
            }

            public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest)
            {
                var requestMethodInfo = ContainerManager.Resolve<IBinarySerializer>().Deserialize<MethodCallInfo>(remotingRequest.Body);
                var classType = Type.GetTypeFromHandle(requestMethodInfo.TypeHandle);
                using (var scope = ContainerManager.BeginLeftScope())
                {
                    var obj = scope.Resolve(classType);
                    try
                    {
                        var result = classType.GetMethod(requestMethodInfo.MethodName, requestMethodInfo.ArgumentTypes)?.Invoke(obj, requestMethodInfo.Parameters);
                        return remotingRequest.CreateSuccessResponse(result);
                    }
                    catch (Exception e)
                    {
                        LogUtil.Error("执行方法失败", e);
                        return remotingRequest.CreateDealErrorResponse();
                    }
                }
            }
        }
    }
}