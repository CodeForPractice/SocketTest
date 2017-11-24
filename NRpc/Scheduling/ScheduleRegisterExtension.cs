using NRpc.Container;

namespace NRpc.Scheduling
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ScheduleRegisterExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 18:03:38
    /// </summary>
    public static class ScheduleRegisterExtension
    {
        /// <summary>
        /// 使用默认的任务调度
        /// </summary>
        /// <param name="containerManager"></param>
        /// <returns></returns>
        public static ContainerManager UseDefaultSchedule(this ContainerManager containerManager)
        {
            containerManager.AddSingleton<IScheduleService, ScheduleService>();
            return containerManager;
        }
    }
}