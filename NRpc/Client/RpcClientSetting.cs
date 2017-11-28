namespace NRpc.Client
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：RpcClientSetting.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/28 16:29:45
    /// </summary>
    public class RpcClientSetting
    {
        public static RpcClientSetting Instance { get; set; } = new RpcClientSetting();

        public int TimeouMillis = 10000;

        public static RpcClientSetting SetInstance(RpcClientSetting rpcClientSetting)
        {
            Instance = rpcClientSetting;
            return Instance;
        }
    }
}