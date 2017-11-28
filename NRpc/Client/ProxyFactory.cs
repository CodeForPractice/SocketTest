namespace NRpc.Client
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ProxyFactory.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/28 16:16:45
    /// </summary>
    public class ProxyFactory
    {
        /// <summary>
        /// 创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>()
        {
            return (T)(new RpcProxyImpl(typeof(T)).GetTransparentProxy());
        }
    }
}