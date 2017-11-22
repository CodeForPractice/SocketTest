using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：MessageFramer.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/22 15:51:06
    /// </summary>
    public class MessageFramer
    {
        public const int HeaderLength = sizeof(int);

        private byte[] _messageBuffer;
        private int _currentHeaderByteSize = 0;
        private int _packageLength = 0;
        private int _currentBufferIndex = 0;
        private Action<ArraySegment<byte>> _receivedHandler;

        public MessageFramer(Action<ArraySegment<byte>> receivedHandler)
        {
            _receivedHandler = receivedHandler;
        }

        public void Parse(IEnumerable<ArraySegment<byte>> dataList)
        {
            if (dataList != null && dataList.Any())
            {
                foreach (var item in dataList)
                {
                    Parse(item);
                }
            }
        }

        public void Parse(ArraySegment<byte> data)
        {
            var messageBytes = data.Array;
            for (int i = data.Offset; i < data.Offset + data.Count; i++)
            {
                if (_currentHeaderByteSize < HeaderLength)
                {
                    _packageLength |= (messageBytes[i] << (_currentHeaderByteSize * 8));// little-endian order
                    ++_currentHeaderByteSize;
                    if (_currentHeaderByteSize == HeaderLength)
                    {
                        if (_packageLength <= 0)
                        {
                            throw new Exception(string.Format("Package length ({0}) is out of bounds.", _packageLength));
                        }
                        _messageBuffer = new byte[_packageLength];
                    }
                }
                else
                {
                    int copyCount = Math.Min(data.Offset + data.Count - i, _packageLength - _currentBufferIndex);
                    try
                    {
                        Buffer.BlockCopy(data.Array, i, _messageBuffer, _currentBufferIndex, copyCount);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error(ex);
                    }
                    _currentBufferIndex += copyCount;
                    i += copyCount - 1;
                    if (_currentBufferIndex == _packageLength)
                    {
                        try
                        {
                            _receivedHandler?.Invoke(new ArraySegment<byte>(_messageBuffer, 0, _currentBufferIndex));
                        }
                        catch (Exception ex)
                        {
                            LogUtil.Error(ex);
                        }
                        _messageBuffer = null;
                        _currentHeaderByteSize = 0;
                        _packageLength = 0;
                        _currentBufferIndex = 0;
                    }
                }
            }
        }
    }
}