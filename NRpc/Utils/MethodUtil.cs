using System;
using System.Reflection;

namespace NRpc.Utils
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：MethodUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：MethodUtil
    /// 创建标识：yjq 2017/11/25 20:18:11
    /// </summary>
    public static class MethodUtil
    {
        /// <summary>
        /// 获取全部的参数类型
        /// </summary>
        /// <param name="arrArgs">参数列表</param>
        /// <returns>全部的参数类型</returns>
        public static Type[] GetArgTypes(object[] arrArgs)
        {
            if (null == arrArgs)
            {
                return new Type[0];
            }
            Type[] result = new Type[arrArgs.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                if (arrArgs[i] == null)
                {
                    result[i] = null;
                }
                else
                {
                    result[i] = arrArgs[i].GetType();
                }
            }
            return result;
        }

        /// <summary>
        /// 获取方法的返回类型
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static MethodType GetMethodType(this MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;
            if (returnType == TypeUtil.AsyncActionType)
                return MethodType.AsyncAction;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == TypeUtil.AsyncFunctionType)
                return MethodType.AsyncFunction;
            if (returnType == TypeUtil.SyncActionType)
            {
                return MethodType.SyncAction;
            }
            return MethodType.SyncFunction;
        }
    }

    /// <summary>
    /// 方法类型
    /// </summary>
    public enum MethodType
    {
        /// <summary>
        /// 同步方法(无返回值)
        /// </summary>
        SyncAction,

        /// <summary>
        /// 同步方法(有返回值)
        /// </summary>
        SyncFunction,

        /// <summary>
        /// 异步(无返回值)
        /// </summary>
        AsyncAction,

        /// <summary>
        /// 异步方法(有返回值)
        /// </summary>
        AsyncFunction
    }
}