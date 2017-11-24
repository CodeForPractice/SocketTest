using System.Collections.Concurrent;

namespace NRpc.Extensions
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ConcurrentDictionaryExtensions.cs
    /// 类属性：公共类（静态）
    /// 类功能描述：ConcurrentDictionaryExtensions
    /// 创建标识：yjq 2017/11/24 21:51:24
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            return dict.TryRemove(key, out value);
        }
    }
}