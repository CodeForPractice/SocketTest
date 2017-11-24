using NRpc.Container;
using NRpc.Transport.Socketing.Framing;

namespace NRpc.Transport
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SocketRegisterExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 17:38:26
    /// </summary>
    public static class SocketRegisterExtension
    {
        /// <summary>
        /// 使用socket
        /// </summary>
        /// <param name="containerManager"></param>
        /// <returns></returns>
        public static ContainerManager UseSocket(this ContainerManager containerManager)
        {
            containerManager.AddSingleton<IMessageFramer, LengthPrefixMessageFramer>();
            return containerManager;
        }
    }
}