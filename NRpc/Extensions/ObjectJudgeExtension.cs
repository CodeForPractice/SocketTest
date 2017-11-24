namespace NRpc.Extensions
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ObjectJudgeExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 15:37:12
    /// </summary>
    internal static partial class ObjectJudgeExtension
    {
        /// <summary>
        /// 为Nll
        /// </summary>
        /// <param name="obj">要判断的值</param>
        /// <returns>为Null时返回true</returns>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// 不为Nll
        /// </summary>
        /// <param name="obj">要判断的值</param>
        /// <returns>不为Null时返回true</returns>
        public static bool IsNotNull(this object obj)
        {
            return !IsNull(obj);
        }

        /// <summary>
        /// 指示指定的字符串是 null 还是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>null或者String.Empty时返回true</returns>
        public static bool IsNullOrEmpty(this object str)
        {
            if (str == null) return true;
            return string.IsNullOrEmpty(str.ToString());
        }

        /// <summary>
        /// 指示指定的字符串不为 null 且不是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>不为 null 且不是String.Empty时返回true</returns>
        public static bool IsNotNullAndNotEmpty(this object str)
        {
            return !(IsNullOrEmpty(str));
        }

        /// <summary>
        /// 字符串是 null、空还是仅由空白字符组成。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>null或者空时返回true</returns>
        public static bool IsNullOrWhiteSpace(this object str)
        {
            if (str == null) return true;
            return string.IsNullOrWhiteSpace(str.ToString());
        }

        /// <summary>
        /// 字符串不是 null、空还是仅由空白字符组成。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>不为null、空时返回true<</returns>
        public static bool IsNotNullAndNotWhiteSpace(this object str)
        {
            return !IsNullOrWhiteSpace(str);
        }
    }
}