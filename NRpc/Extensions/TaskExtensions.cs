using System;
using System.Threading;
using System.Threading.Tasks;

namespace NRpc.Extensions
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：TaskExtensions.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：TaskExtensions
    /// 创建标识：yjq 2017/11/24 21:59:56
    /// </summary>
    public static class TaskExtensions
    {
        public static TResult WaitResult<TResult>(this Task<TResult> task, int timeoutMillis)
        {
            if (task.Wait(timeoutMillis))
            {
                return task.Result;
            }
            return default(TResult);
        }

        public static async Task TimeoutAfter(this Task task, int millisecondsDelay)
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsDelay, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsDelay)
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsDelay, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return task.Result;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}