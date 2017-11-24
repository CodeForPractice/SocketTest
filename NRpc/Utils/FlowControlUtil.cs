namespace NRpc.Utils
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：FlowControlUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 17:44:01
    /// </summary>
    public static class FlowControlUtil
    {
        public static int CalculateFlowControlTimeMilliseconds(int pendingCount, int thresholdCount, int stepPercent, int baseWaitMilliseconds, int maxWaitMilliseconds = 10000)
        {
            var exceedCount = pendingCount - thresholdCount;
            exceedCount = exceedCount <= 0 ? 1 : exceedCount;

            var stepCount = stepPercent * thresholdCount / 100;
            stepCount = stepCount <= 0 ? 1 : stepCount;

            var times = exceedCount / stepCount;
            times = times <= 0 ? 1 : times;

            var waitMilliseconds = times * baseWaitMilliseconds;

            if (waitMilliseconds > maxWaitMilliseconds)
            {
                return maxWaitMilliseconds;
            }
            return waitMilliseconds;
        }
    }
}