using NRpc.Container;
using NRpc.Proxy;
using NRpc.Serializing;
using NRpc.Transport.Remoting;
using NRpc.Utils;
using System;

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
        public NRpcHandle()
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