﻿namespace NRpc.Logger.NLogger
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：NLoggerFactory.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 15:54:01
    /// </summary>
    internal sealed class NLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// 根据loggerName创建NLogLogger
        /// </summary>
        /// <param name="loggerName">logger名字</param>
        /// <returns>NLogLogger</returns>
        public ILogger Create(string loggerName)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(loggerName));
        }
    }
}