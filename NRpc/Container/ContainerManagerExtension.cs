using NRpc.Logger;
using NRpc.Proxy;
using NRpc.Scheduling;
using NRpc.Serializing;
using NRpc.Transport;

namespace NRpc.Container
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ContainerManagerExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：ContainerManagerExtension
    /// 创建标识：yjq 2017/11/24 22:06:45
    /// </summary>
    public static class ContainerManagerExtension
    {
        public static ContainerManager UseDefault(this ContainerManager containerManager)
        {
            containerManager.UseNLog()
                            .UseDefaultSchedule()
                            .UseSocket()
                            .UseDefaultBinarySerialize();

            RemotingClientFactory.RegisterUnLoad();
            return containerManager;
        }
    }
}