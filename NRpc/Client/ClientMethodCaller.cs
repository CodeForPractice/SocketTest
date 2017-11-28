using NRpc.Container;
using NRpc.Serializing;
using NRpc.Transport.Remoting;
using NRpc.Utils;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRpc.Client
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ClientMethodCaller.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/28 16:12:08
    /// </summary>
    public class ClientMethodCaller
    {
        /// <summary>
        /// 异步方法处理
        /// </summary>
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(ClientMethodCaller).GetMethod("AsyncFunction", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IBinarySerializer _binarySerializer;
        private readonly Type _proxyType;

        public ClientMethodCaller(Type proxyType)
        {
            _proxyType = proxyType;
            _binarySerializer = ContainerManager.Resolve<IBinarySerializer>();
        }

        /// <summary>
        /// 发送远程请求
        /// </summary>
        /// <param name="arrMethodArgs"></param>
        /// <param name="argmentTypes"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public object DoMethodCall(object[] arrMethodArgs, Type[] argmentTypes, MethodInfo methodInfo)
        {
            var client = RemotingClientFactory.GetClient(_proxyType);
            var requestInfo = Create(arrMethodArgs, argmentTypes, methodInfo);
            var response = client.InvokeSync(requestInfo, RpcClientSetting.Instance.TimeouMillis);
            return HandleResponse(response, methodInfo);
        }

        private RemotingRequest Create(object[] arrMethodArgs, Type[] argmentTypes, MethodInfo methodInfo)
        {
            var methodCallInfo = new MethodCallInfo()
            {
                ClassName = _proxyType.FullName,
                MethodName = methodInfo.Name,
                Parameters = arrMethodArgs,
                TypeHandle = _proxyType.TypeHandle,
                ArgumentTypes = argmentTypes
            };
            var body = _binarySerializer.Serialize(methodCallInfo);
            return new RemotingRequest(100, body);
        }

        /// <summary>
        /// 处理反馈结果
        /// </summary>
        /// <param name="remotingResponse"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public object HandleResponse(RemotingResponse remotingResponse, MethodInfo methodInfo)
        {
            if (remotingResponse.ResponseCode != ResponseState.Success)
            {
                throw new Exception(Encoding.UTF8.GetString(remotingResponse.ResponseBody));
            }
            var delegateType = methodInfo.GetMethodType();
            if (delegateType == MethodType.SyncAction)
            {
                return null;
            }
            else if (delegateType == MethodType.SyncFunction)
            {
                return GetSyncFunctionResult(remotingResponse, methodInfo);
            }
            else if (delegateType == MethodType.AsyncAction)
            {
                return AsyncAction();
            }
            else
            {
                return GetAsyncFunctionResult(remotingResponse, methodInfo);
            }
        }

        /// <summary>
        /// 获取同步方法的返回值
        /// </summary>
        /// <param name="remotingResponse"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private object GetSyncFunctionResult(RemotingResponse remotingResponse, MethodInfo methodInfo)
        {
            if (remotingResponse.IsEmptyBody())
            {
                return null;
            }
            else
            {
                return _binarySerializer.Deserialize(remotingResponse.ResponseBody, methodInfo.ReturnType);
            }
        }

        /// <summary>
        /// 获取异步方法的返回值
        /// </summary>
        /// <param name="remotingResponse"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private object GetAsyncFunctionResult(RemotingResponse remotingResponse, MethodInfo methodInfo)
        {
            var resultType = methodInfo.ReturnType.GetGenericArguments()[0];
            var mi = HandleAsyncMethodInfo.MakeGenericMethod(resultType);
            if (remotingResponse.IsEmptyBody())
            {
                return mi.Invoke(this, new[] { null as object });
            }
            var executeResult = _binarySerializer.Deserialize(remotingResponse.ResponseBody, resultType);
            var result = mi.Invoke(this, new[] { executeResult });
            return result;
        }

        /// <summary>
        /// 异步无返回值方法
        /// </summary>
        /// <returns></returns>
        private Task AsyncAction()
        {
            return Task.Delay(1);
        }

        /// <summary>
        /// 异步有返回值方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private Task<T> AsyncFunction<T>(T value)
        {
            return Task.FromResult(value);
        }
    }
}