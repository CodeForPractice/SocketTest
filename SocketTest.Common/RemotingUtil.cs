using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：RemotingUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 11:08:48
    /// </summary>
    public static class RemotingUtil
    {
        public static byte[] BuilderRequestMessage(this SocketRequest socketRequest)
        {
            socketRequest.Id.EncodeString(out byte[] idLengthBytes, out byte[] idBytes);
            var codeBytes = BitConverter.GetBytes(socketRequest.Code);
            var typeBytes = BitConverter.GetBytes(socketRequest.Type);
            var createdTimeBytes = ByteUtil.EncodeDateTime(socketRequest.CreatedTime);
            var headerBytes = HeaderUtil.EncodeHeader(socketRequest.Header);
            var headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);
            return ByteUtil.Combine(idLengthBytes, idBytes, codeBytes, typeBytes, createdTimeBytes, headerLengthBytes, headerBytes, socketRequest.Body);
        }

        public static SocketRequest ParseRequest(this byte[] data)
        {
            var currentOffset = 0;
            var id = data.DecodeString(currentOffset, out currentOffset);
            var code = data.DecodeShort(currentOffset, out currentOffset);
            var type = data.DecodeShort(currentOffset, out currentOffset);
            var createdTime = data.DecodeDateTime(currentOffset, out currentOffset);
            var headerLength = data.DecodeInt(currentOffset, out currentOffset);
            var header = data.DecodeHeader(currentOffset, out currentOffset);
            var bodyLength = data.Length - currentOffset;
            var body = new byte[bodyLength];
            Buffer.BlockCopy(data, currentOffset, body, 0, bodyLength);
            return new SocketRequest(id, code, body, createdTime, header) { Type = type };
        }

        public static byte[] BuildResponseMessage(this SocketResponse response)
        {
            response.RequestId.EncodeString(out byte[] idLengthBytes, out byte[] idBytes);
            var requestCodeBytes = BitConverter.GetBytes(response.RequestCode);
            var requestTypeBytes = BitConverter.GetBytes(response.RequestType);
            var requestTimeBytes = ByteUtil.EncodeDateTime(response.RequestTime);
            var requestHeaderBytes = HeaderUtil.EncodeHeader(response.RequestHeader);
            var requestHeaderLengthBytes = BitConverter.GetBytes(requestHeaderBytes.Length);

            var responseCodeBytes = BitConverter.GetBytes(response.ResponseCode);
            var responseTimeBytes = ByteUtil.EncodeDateTime(response.ResponseTime);
            var responseHeaderBytes = HeaderUtil.EncodeHeader(response.ResponseHeader);
            var responseHeaderLengthBytes = BitConverter.GetBytes(requestHeaderBytes.Length);

            return ByteUtil.Combine(idLengthBytes, idBytes, requestCodeBytes, requestTypeBytes, requestTimeBytes, requestHeaderLengthBytes, requestHeaderBytes, responseCodeBytes, responseTimeBytes, responseHeaderLengthBytes, responseHeaderBytes, response.ResponseBody);
        }

        public static SocketResponse ParseResponse(this byte[] data)
        {
            var currentOffset = 0;
            var requestId = data.DecodeString(currentOffset, out currentOffset);
            var requestCode = ByteUtil.DecodeShort(data, currentOffset, out currentOffset);
            var requestType = ByteUtil.DecodeShort(data, currentOffset, out currentOffset);
            var requestTime = ByteUtil.DecodeDateTime(data, currentOffset, out currentOffset);
            var requestHeaderLength = ByteUtil.DecodeInt(data, currentOffset, out currentOffset);
            var requestHeader = HeaderUtil.DecodeHeader(data, currentOffset, out currentOffset);
            var responseCode = ByteUtil.DecodeShort(data, currentOffset, out currentOffset);
            var responseTime = ByteUtil.DecodeDateTime(data, currentOffset, out currentOffset);
            var responseHeaderLength = ByteUtil.DecodeInt(data, currentOffset, out currentOffset);
            var responseHeader = HeaderUtil.DecodeHeader(data, currentOffset, out currentOffset);

            var responseBodyLength = data.Length - currentOffset;
            var responseBody = new byte[responseBodyLength];
            Buffer.BlockCopy(data, currentOffset, responseBody, 0, responseBodyLength);
            return new SocketResponse(requestType, requestCode, requestId, requestTime, responseCode, responseBody, responseTime, requestHeader, responseHeader);
        }
    }

    public static class HeaderUtil
    {
        public static readonly byte[] ZeroLengthBytes = BitConverter.GetBytes(0);
        public static readonly byte[] EmptyBytes = new byte[0];

        public static byte[] EncodeHeader(this IDictionary<string, string> header)
        {
            var headerKeyCount = header != null ? header.Count : 0;
            var headerKeyCountBytes = BitConverter.GetBytes(headerKeyCount);
            var bytesList = new List<byte[]>();

            bytesList.Add(headerKeyCountBytes);
            if (headerKeyCount > 0)
            {
                foreach (var item in header)
                {
                    byte[] valueBytes;
                    byte[] valueLengthBytes;
                    ByteUtil.EncodeString(item.Key, out byte[] keyLengthBytes, out byte[] keyBytes);
                    ByteUtil.EncodeString(item.Value, out valueLengthBytes, out valueBytes);
                    bytesList.Add(keyLengthBytes);
                    bytesList.Add(keyBytes);
                    bytesList.Add(valueLengthBytes);
                    bytesList.Add(valueBytes);
                }
            }
            return bytesList.CombineToByte();
        }

        public static IDictionary<string, string> DecodeHeader(this byte[] data, int startOffset, out int nextStartOffset)
        {
            var dict = new Dictionary<string, string>();
            var srcOffset = startOffset;
            var headerKeyCount = ByteUtil.DecodeInt(data, srcOffset, out srcOffset);
            for (var i = 0; i < headerKeyCount; i++)
            {
                var key = ByteUtil.DecodeString(data, srcOffset, out srcOffset);
                var value = ByteUtil.DecodeString(data, srcOffset, out srcOffset);
                dict.Add(key, value);
            }
            nextStartOffset = srcOffset;
            return dict;
        }
    }
}
