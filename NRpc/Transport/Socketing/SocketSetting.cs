namespace NRpc.Transport.Socketing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SocketSetting.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：Socket设置
    /// 创建标识：yjq 2017/11/24 16:24:42
    /// </summary>
    public class SocketSetting
    {
        private int _sendBufferSize = 1024 * 16;
        private int _receiveBufferSize = 1024 * 16;
        private int _sendPackageSize = 1024 * 64;

        public virtual int SendBufferSize
        {
            get
            {
                return _sendBufferSize;
            }
        }

        public virtual int ReceiveBufferSize
        {
            get
            {
                return _receiveBufferSize;
            }
        }

        public virtual int SendPackageSize
        {
            get
            {
                return _sendPackageSize;
            }
        }

        public virtual int SendMessageFlowControlThreshold
        {
            get { return 1000; }
        }

        public virtual int SendMessageFlowControlStepPercent
        {
            get { return 1; }
        }

        public virtual int SendMessageFlowControlWaitMilliseconds
        {
            get { return 1; }
        }
    }
}