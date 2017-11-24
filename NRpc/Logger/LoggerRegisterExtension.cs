using NRpc.Container;
using NRpc.Logger.NLogger;

namespace NRpc.Logger
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：LoggerRegisterExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 16:02:06
    /// </summary>
    public static class LoggerRegisterExtension
    {
        /// <summary>
        /// 使用Nlog
        /// </summary>
        /// <param name="containerManager"></param>
        /// <returns></returns>
        public static ContainerManager UseNLog(this ContainerManager containerManager)
        {
            containerManager.AddSingleton<ILoggerFactory, NLoggerFactory>();
            return containerManager;
        }
    }
}