﻿using NRpc.Container;
using NRpc.Serializing;
using NRpc.Transport.Remoting;
using NRpc.Utils;
using System;
using System.Text;

namespace NRpc.Proxy
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：DefalutResponse.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/27 17:43:02
    /// </summary>
    public static class ResponseUtil
    {
        public static readonly byte[] DealErrorResponseBody = Encoding.UTF8.GetBytes("Remoting Deal Error.");

        /// <summary>
        /// 创建处理出错的RemotingResponse
        /// </summary>
        /// <param name="remotingRequest"></param>
        /// <returns></returns>
        public static RemotingResponse CreateDealErrorResponse(this RemotingRequest remotingRequest)
        {
            return new RemotingResponse(remotingRequest.Type, remotingRequest.Code, remotingRequest.Sequence, remotingRequest.CreatedTime, 500, DealErrorResponseBody, DateTime.Now, remotingRequest.Header, null);
        }

        /// <summary>
        /// 创建成功的输出对象
        /// </summary>
        /// <param name="remotingRequest"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static RemotingResponse CreateSuccessResponse(this RemotingRequest remotingRequest, object result)
        {
            byte[] body = null;
            if (result != null && result.GetType() != TypeUtil.ByteArrayType)
            {
                var _binarySerializer = ContainerManager.Resolve<IBinarySerializer>();
                body = _binarySerializer.Serialize(result);
            }
            else
            {
                body = result as byte[];
            }

            return new RemotingResponse(remotingRequest.Type, remotingRequest.Code, remotingRequest.Sequence, remotingRequest.CreatedTime, 100, body, DateTime.Now, remotingRequest.Header, null);
        }
    }
}