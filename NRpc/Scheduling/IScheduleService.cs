using System;

namespace NRpc.Scheduling
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：IScheduleService.cs
    /// 接口属性：公共
    /// 类功能描述：IScheduleService接口
    /// 创建标识：yjq 2017/11/24 17:59:16
    /// </summary>
    public interface IScheduleService
    {
        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="name">任务名字</param>
        /// <param name="action">执行方法</param>
        /// <param name="dueTime">延迟时间</param>
        /// <param name="period"></param>
        void StartTask(string name, Action action, int dueTime, int period);

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="name">任务名字</param>
        void StopTask(string name);
    }
}