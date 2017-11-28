using NRpc.Client;
using NRpc.Container;
using NRpc.Serializing;
using NRpc.Transport.Remoting;
using NRpc.Utils;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NRpc.Server
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ServerMethodCaller.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/28 14:31:58
    /// </summary>
    public class ServerMethodCaller
    {
        /// <summary>
        /// 异步方法处理
        /// </summary>
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(ServerMethodCaller).GetMethod("ExecuteAsyncResultAction", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 请求处理
        /// </summary>
        /// <param name="remotingRequest"></param>
        /// <returns></returns>
        public RemotingResponse HandleRequest(RemotingRequest remotingRequest)
        {
            var requestMethodInfo = ContainerManager.Resolve<IBinarySerializer>().Deserialize<MethodCallInfo>(remotingRequest.Body);
            var classType = Type.GetTypeFromHandle(requestMethodInfo.TypeHandle);
            using (var scope = ContainerManager.BeginLeftScope())
            {
                var obj = scope.Resolve(classType);
                try
                {
                    var executeMethodInfo = classType.GetMethod(requestMethodInfo.MethodName, requestMethodInfo.ArgumentTypes);
                    if (executeMethodInfo == null)
                    {
                        return remotingRequest.CreateNotFoundResponse($"{requestMethodInfo.ClassName},{requestMethodInfo.MethodName}");
                    }
                    else
                    {
                        var delegateType = executeMethodInfo.GetMethodType();
                        var executeResult = executeMethodInfo.Invoke(obj, requestMethodInfo.Parameters);
                        if (delegateType == MethodType.SyncAction)
                        {
                            return remotingRequest.CreateSuccessResponse(ResponseUtil.NoneBodyResponse);
                        }
                        else if (delegateType == MethodType.SyncFunction)
                        {
                            return remotingRequest.CreateSuccessResponse(executeResult);
                        }
                        else if (delegateType == MethodType.AsyncAction)
                        {
                            var task = (Task)executeResult;
                            task.Wait();
                            return remotingRequest.CreateSuccessResponse(ResponseUtil.NoneBodyResponse);
                        }
                        else
                        {
                            var resultType = executeMethodInfo.ReturnType.GetGenericArguments()[0];
                            var mi = HandleAsyncMethodInfo.MakeGenericMethod(resultType);
                            var result = mi.Invoke(this, new[] { executeResult });
                            return remotingRequest.CreateSuccessResponse(result);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogUtil.Error("执行方法失败", e);
                    return remotingRequest.CreateDealErrorResponse();
                }
            }
        }

        /// <summary>
        /// 执行异步有返回值的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        private T ExecuteAsyncResultAction<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}