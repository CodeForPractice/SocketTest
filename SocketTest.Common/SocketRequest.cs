using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SocketRequest.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 10:33:42
    /// </summary>
    public class SocketRequest
    {
        /// <summary>
        /// 请求ID,唯一不重复
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        public short Type { get; set; }

        /// <summary>
        /// 请求状态码
        /// </summary>
        public short Code { get; set; }

        /// <summary>
        ///
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        public IDictionary<string, string> Header { get; set; }

        public SocketRequest()
        {
        }

        public SocketRequest(short code, byte[] body, IDictionary<string, string> header = null) : this(ObjectId.GenerateNewStringId(), code, body, DateTime.Now, header) { }
        public SocketRequest(string id, short code, byte[] body, DateTime createdTime, IDictionary<string, string> header)
        {
            Id = id;
            Code = code;
            Body = body;
            Header = header;
            CreatedTime = createdTime;
        }

        public override string ToString()
        {
            var createdTime = CreatedTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var bodyLength = 0;
            if (Body != null)
            {
                bodyLength = Body.Length;
            }
            var header = string.Empty;
            if (Header != null && Header.Count > 0)
            {
                header = string.Join(",", Header.Select(x => string.Format("{0}:{1}", x.Key, x.Value)));
            }
            return string.Format("[Id:{0}, Type:{1}, Code:{2},  CreatedTime:{3}, BodyLength:{4}, Header: [{5}]]",
                Id, Type, Code, createdTime, bodyLength, header);
        }
    }

    public class SocketRequestType
    {
        public const short Async = 1;
        public const short Oneway = 2;
        public const short Callback = 3;
    }
}