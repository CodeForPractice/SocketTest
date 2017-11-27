using NRpc.Container;

namespace NRpc.Serializing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：BinarySerializeRegisterExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/27 17:18:27
    /// </summary>
    public static class BinarySerializeRegisterExtension
    {
        /// <summary>
        /// 使用默认的字节数组序列化
        /// </summary>
        /// <param name="containerManager"></param>
        /// <returns></returns>
        public static ContainerManager UseDefaultBinarySerialize(this ContainerManager containerManager)
        {
            containerManager.AddSingleton<IBinarySerializer, DefaultBinarySerializer>();
            return containerManager;
        }
    }
}