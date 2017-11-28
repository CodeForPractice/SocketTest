using System;

namespace NRpc.Client
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：MethodCallInfo.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/28 16:16:11
    /// </summary>
    [Serializable]
    internal class MethodCallInfo
    {
        public MethodCallInfo()
        {
        }

        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public object[] Parameters { get; set; }

        public RuntimeTypeHandle TypeHandle { get; set; }

        public Type[] ArgumentTypes { get; set; }
    }
}