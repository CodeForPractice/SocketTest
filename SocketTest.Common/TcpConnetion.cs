using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：TcpConnetion.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/22 13:33:50
    /// </summary>
    public class TcpConnetion : ITcpConnection
    {
        private Socket _socket;

        private readonly EndPoint _localEndPoint;
        private readonly EndPoint _remotingEndPoint;

        private ConcurrentQueue<IEnumerable<ArraySegment<byte>>> _sendingMessageQueue = new ConcurrentQueue<IEnumerable<ArraySegment<byte>>>();
        private MemoryStream _sendingStream;
        private SocketAsyncEventArgs _sendAsyncEventArgs;

        private ConcurrentQueue<ArraySegment<byte>> _receiveMessageQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private SocketAsyncEventArgs _receiveEventArgs;

        private Action<TcpConnetion, byte[]> _messageHandle;

        private SocketSetting _socketSetting;

        private int _sending = 0;
        private int _receiving = 0;
        private int _closing = 0;
        private int _parsing = 0;
        private MessageFramer _messageFramer;

        private int _receivedMessageCount = 0;

        public TcpConnetion(Socket socket, Action<TcpConnetion, byte[]> messageHandle, SocketSetting socketSetting)
        {
            _socket = socket;
            _socketSetting = socketSetting;
            _localEndPoint = socket.LocalEndPoint;
            _remotingEndPoint = socket.RemoteEndPoint;
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.AcceptSocket = socket;
            _receiveEventArgs.Completed += ReceiveEventArgs_Completed;

            _sendAsyncEventArgs = new SocketAsyncEventArgs();
            _sendAsyncEventArgs.AcceptSocket = socket;
            _sendAsyncEventArgs.Completed += SendAsyncEventArgs_Completed;

            _sendingStream = new MemoryStream();
            _messageFramer = new MessageFramer(OnMessageArrived);
            _messageHandle = messageHandle;
            TryReceive();
        }

        public bool IsConnected
        {
            get { return _socket != null && _socket.Connected; }
        }

        public Socket Socket
        {
            get { return _socket; }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                return _localEndPoint;
            }
        }

        public EndPoint RemotingEndPoint
        {
            get
            {
                return _remotingEndPoint;
            }
        }

        private void ReceiveEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success)
            {
                Close();
            }
            else
            {
                var segmentList = new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred);
                _receiveMessageQueue.Enqueue(segmentList);
                e.SetBuffer(null, 0, 0);
                ParseReceiveData();
            }
            ExitReceive();
            TryReceive();
        }

        private void TryReceive()
        {
            if (EnterReceive())
            {
                try
                {
                    _receiveEventArgs.SetBuffer(new byte[_socketSetting.ReceiveBufferSize], 0, _socketSetting.ReceiveBufferSize);
                    if (!_receiveEventArgs.AcceptSocket.ReceiveAsync(_receiveEventArgs))
                    {
                        ProcessReceive(_receiveEventArgs);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex);
                    Close();
                }
            }
        }

        private bool EnterReceive()
        {
            var flag = Interlocked.CompareExchange(ref _receiving, 1, 0) == 0;
            //if (!flag)
            //{
            //    LogUtil.Debug("进入接收失败");
            //}
            return flag;
        }

        private void ExitReceive()
        {
            Interlocked.Exchange(ref _receiving, 0);
        }

        private void ParseReceiveData()
        {
            if (EnterParse())
            {
                try
                {
                    var segmentList = new List<ArraySegment<byte>>();
                    while (_receiveMessageQueue.TryDequeue(out ArraySegment<byte> data))
                    {
                        segmentList.Add(data);
                    }
                    _messageFramer.Parse(segmentList);
                }
                finally
                {
                    ExitParse();
                }
            }
        }

        private void OnMessageArrived(ArraySegment<byte> messageSegment)
        {
            Interlocked.Increment(ref _receivedMessageCount);
            LogUtil.Fatal($"当前已接收{_receivedMessageCount.ToString()}条记录");
            byte[] message = new byte[messageSegment.Count];
            Array.Copy(messageSegment.Array, messageSegment.Offset, message, 0, messageSegment.Count);
            try
            {
                _messageHandle?.Invoke(this, message);
                //LogUtil.Info($"接收来自客户端发送的信息:{_remotingEndPoint}:【{Encoding.UTF8.GetString(message)}】");
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
            }
        }

        private bool EnterParse()
        {
            var flag = Interlocked.CompareExchange(ref _parsing, 1, 0) == 0;
            //if (!flag)
            //{
            //    LogUtil.Debug("进入解析失败");
            //}
            return flag;
        }

        private void ExitParse()
        {
            Interlocked.Exchange(ref _parsing, 0);
        }

        public void SendMessage(byte[] messageBytes)
        {
            if (messageBytes == null || messageBytes.Length <= 0)
            {
                return;
            }
            var segmentList = FramerUtil.Packet(messageBytes);
            _sendingMessageQueue.Enqueue(segmentList);
            TrySend();
        }

        private void SendAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.Buffer != null)
            {
                e.SetBuffer(null, 0, 0);
            }
            ExitSending();

            if (e.SocketError == SocketError.Success)
            {
                LogUtil.Info("消息发送成功");
                TrySend();
            }
            else
            {
                Close();
            }
        }

        private void TrySend()
        {
            if (_closing == 1)
            {
                return;
            }
            if (EnterSending())
            {
                _sendingStream.SetLength(0);
                IEnumerable<ArraySegment<byte>> sendData;
                int currentStreamLength = 0;
                while (_sendingMessageQueue.TryDequeue(out sendData))
                {
                    foreach (var item in sendData)
                    {
                        _sendingStream.Write(item.Array, item.Offset, item.Count);
                        currentStreamLength += item.Count;
                    }
                    if (currentStreamLength >= _socketSetting.SendBufferSize)
                    {
                        break;
                    }
                }
                if (currentStreamLength == 0)
                {
                    ExitSending();
                    if (_sendingMessageQueue.Count > 0)
                    {
                        TrySend();
                    }
                    return;
                }
                try
                {
                    _sendAsyncEventArgs.SetBuffer(_sendingStream.GetBuffer(), 0, currentStreamLength);
                    if (!_sendAsyncEventArgs.AcceptSocket.SendAsync(_sendAsyncEventArgs))
                    {
                        ProcessSend(_sendAsyncEventArgs);
                    }
                }
                catch (Exception e)
                {
                    LogUtil.Error(e);
                    Close();
                    ExitSending();
                }
            }
        }

        private bool EnterSending()
        {
            var flag = Interlocked.CompareExchange(ref _sending, 1, 0) == 0;
            //LogUtil.Debug(flag.ToString());
            //LogUtil.Debug(_sending.ToString());
            return flag;
        }

        private void ExitSending()
        {
            Interlocked.Exchange(ref _sending, 0);
        }

        public void Close()
        {
            if (Interlocked.CompareExchange(ref _closing, 1, 0) == 0)
            {
                LogUtil.Info($"客户端关闭:{_remotingEndPoint}");
                _receiveEventArgs?.DisposeCurrent(ReceiveEventArgs_Completed);
                _sendAsyncEventArgs?.DisposeCurrent(SendAsyncEventArgs_Completed);
                _socket.ShutDownCurrent();
            }
        }
    }
}