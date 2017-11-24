using NRpc.Container;
using NRpc.Transport.Socketing.Framing;
using NRpc.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NRpc.Transport.Socketing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：TcpConnection.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 16:26:37
    /// </summary>
    public class TcpConnection : ITcpConnection
    {
        #region Private Properties

        private Socket _socket;
        private readonly SocketSetting _setting;
        private readonly EndPoint _localEndPoint;
        private readonly EndPoint _remotingEndPoint;
        private readonly SocketAsyncEventArgs _sendSocketArgs;
        private readonly SocketAsyncEventArgs _receiveSocketArgs;
        private readonly IMessageFramer _framer;
        private readonly ConcurrentQueue<IEnumerable<ArraySegment<byte>>> _sendingQueue = new ConcurrentQueue<IEnumerable<ArraySegment<byte>>>();
        private readonly ConcurrentQueue<ArraySegment<byte>> _receiveQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private readonly MemoryStream _sendingStream = new MemoryStream();

        private Action<ITcpConnection, SocketError> _connectionClosedHandler;
        private Action<ITcpConnection, byte[]> _messageArrivedHandler;

        private int _sending;
        private int _receiving;
        private int _parsing;
        private int _closing;

        private long _pendingMessageCount;

        #endregion Private Properties

        #region Public Properties

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
            get { return _localEndPoint; }
        }

        public EndPoint RemotingEndPoint
        {
            get { return _remotingEndPoint; }
        }

        public SocketSetting Setting
        {
            get { return _setting; }
        }

        public long PendingMessageCount
        {
            get { return _pendingMessageCount; }
        }

        #endregion Public Properties

        public TcpConnection(Socket socket, SocketSetting setting, Action<ITcpConnection, byte[]> messageArrivedHandler, Action<ITcpConnection, SocketError> connectionClosedHandler)
        {
            Ensure.NotNull(socket, "socket");
            Ensure.NotNull(setting, "setting");
            Ensure.NotNull(messageArrivedHandler, "messageArrivedHandler");
            Ensure.NotNull(connectionClosedHandler, "connectionClosedHandler");

            _socket = socket;
            _setting = setting;
            _localEndPoint = socket.LocalEndPoint;
            _remotingEndPoint = socket.RemoteEndPoint;
            _messageArrivedHandler = messageArrivedHandler;
            _connectionClosedHandler = connectionClosedHandler;

            _sendSocketArgs = new SocketAsyncEventArgs();
            _sendSocketArgs.AcceptSocket = socket;
            _sendSocketArgs.Completed += OnSendAsyncCompleted;

            _receiveSocketArgs = new SocketAsyncEventArgs();
            _receiveSocketArgs.AcceptSocket = socket;
            _receiveSocketArgs.Completed += OnReceiveAsyncCompleted;

            _framer = ContainerManager.Resolve<IMessageFramer>();
            _framer.RegisterMessageArrivedCallback(OnMessageArrived);
        }

        public void SendMessage(byte[] message)
        {
            if (message.Length == 0)
            {
                return;
            }

            var segments = _framer.FrameData(new ArraySegment<byte>(message, 0, message.Length));
            _sendingQueue.Enqueue(segments);
            Interlocked.Increment(ref _pendingMessageCount);

            TrySend();
        }

        public void Close()
        {
            CloseInternal(SocketError.Success, "Socket normal close.", null);
        }

        #region Send Methods

        private void TrySend()
        {
            if (_closing == 1) return;
            if (!EnterSending()) return;

            _sendingStream.SetLength(0);

            IEnumerable<ArraySegment<byte>> segments;
            while (_sendingQueue.TryDequeue(out segments))
            {
                Interlocked.Decrement(ref _pendingMessageCount);
                foreach (var segment in segments)
                {
                    _sendingStream.Write(segment.Array, segment.Offset, segment.Count);
                }
                if (_sendingStream.Length >= _setting.SendPackageSize)
                {
                    break;
                }
            }

            if (_sendingStream.Length == 0)
            {
                ExitSending();
                if (_sendingQueue.Count > 0)
                {
                    TrySend();
                }
                return;
            }

            try
            {
                _sendSocketArgs.SetBuffer(_sendingStream.GetBuffer(), 0, (int)_sendingStream.Length);
                var firedAsync = _sendSocketArgs.AcceptSocket.SendAsync(_sendSocketArgs);
                if (!firedAsync)
                {
                    ProcessSend(_sendSocketArgs);
                }
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "Socket send error, errorMessage:" + ex.Message, ex);
                ExitSending();
            }
        }

        private void ProcessSend(SocketAsyncEventArgs socketArgs)
        {
            if (_closing == 1) return;
            if (socketArgs.Buffer != null)
            {
                socketArgs.SetBuffer(null, 0, 0);
            }

            ExitSending();

            if (socketArgs.SocketError == SocketError.Success)
            {
                TrySend();
            }
            else
            {
                CloseInternal(socketArgs.SocketError, "Socket send error.", null);
            }
        }

        private void OnSendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        private bool EnterSending()
        {
            return Interlocked.CompareExchange(ref _sending, 1, 0) == 0;
        }

        private void ExitSending()
        {
            Interlocked.Exchange(ref _sending, 0);
        }

        #endregion Send Methods

        #region Receive Methods

        private void TryReceive()
        {
            if (!EnterReceiving()) return;
            try
            {
                var buffer = new Byte[_setting.ReceiveBufferSize];
                _receiveSocketArgs.SetBuffer(buffer, 0, buffer.Length);
                if (_receiveSocketArgs.Buffer == null)
                {
                    CloseInternal(SocketError.Shutdown, "Socket receive set buffer failed.", null);
                    ExitReceiving();
                    return;
                }

                bool firedAsync = _receiveSocketArgs.AcceptSocket.ReceiveAsync(_receiveSocketArgs);
                if (!firedAsync)
                {
                    ProcessReceive(_receiveSocketArgs);
                }
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "Socket receive error, errorMessage:" + ex.Message, ex);
                ExitReceiving();
            }
        }

        private void OnReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs socketArgs)
        {
            if (socketArgs.BytesTransferred == 0 || socketArgs.SocketError != SocketError.Success)
            {
                CloseInternal(socketArgs.SocketError, socketArgs.SocketError != SocketError.Success ? "Socket receive error" : "Socket normal close", null);
                return;
            }

            try
            {
                var segment = new ArraySegment<byte>(socketArgs.Buffer, socketArgs.Offset, socketArgs.Count);
                _receiveQueue.Enqueue(segment);
                socketArgs.SetBuffer(null, 0, 0);

                TryParsingReceivedData();
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "Parsing received data error.", ex);
                return;
            }

            ExitReceiving();
            TryReceive();
        }

        private void TryParsingReceivedData()
        {
            if (!EnterParsing()) return;

            try
            {
                var segmentList = new List<ArraySegment<byte>>();

                while (_receiveQueue.TryDequeue(out ArraySegment<byte> data))
                {
                    segmentList.Add(data);
                }

                _framer.Package(segmentList);
            }
            finally
            {
                ExitParsing();
            }
        }

        private void OnMessageArrived(ArraySegment<byte> messageSegment)
        {
            byte[] message = new byte[messageSegment.Count];
            Array.Copy(messageSegment.Array, messageSegment.Offset, message, 0, messageSegment.Count);
            try
            {
                _messageArrivedHandler(this, message);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Call message arrived handler failed.", ex);
            }
        }

        private bool EnterReceiving()
        {
            return Interlocked.CompareExchange(ref _receiving, 1, 0) == 0;
        }

        private void ExitReceiving()
        {
            Interlocked.Exchange(ref _receiving, 0);
        }

        private bool EnterParsing()
        {
            return Interlocked.CompareExchange(ref _parsing, 1, 0) == 0;
        }

        private void ExitParsing()
        {
            Interlocked.Exchange(ref _parsing, 0);
        }

        private struct ReceivedData
        {
            public readonly ArraySegment<byte> Buf;
            public readonly int DataLen;

            public ReceivedData(ArraySegment<byte> buf, int dataLen)
            {
                Buf = buf;
                DataLen = dataLen;
            }
        }

        #endregion Receive Methods

        private void CloseInternal(SocketError socketError, string reason, Exception exception)
        {
            if (Interlocked.CompareExchange(ref _closing, 1, 0) == 0)
            {
                ExceptionUtil.Eat(() =>
                {
                    if (_sendSocketArgs != null)
                    {
                        _sendSocketArgs.Completed -= OnSendAsyncCompleted;
                        _sendSocketArgs.AcceptSocket = null;
                        _sendSocketArgs.Dispose();
                    }
                    if (_receiveSocketArgs != null)
                    {
                        _receiveSocketArgs.Completed -= OnReceiveAsyncCompleted;
                        _receiveSocketArgs.AcceptSocket = null;
                        _receiveSocketArgs.Dispose();
                    }
                });

                SocketUtils.ShutdownSocket(_socket);
                var isDisposedException = exception != null && exception is ObjectDisposedException;
                if (!isDisposedException)
                {
                    LogUtil.Info(string.Format("Socket closed, remote endpoint:{0} socketError:{1}, reason:{2}, ex:{3}", RemotingEndPoint, socketError, reason, exception));
                }
                _socket = null;

                if (_connectionClosedHandler != null)
                {
                    try
                    {
                        _connectionClosedHandler(this, socketError);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error("Call connection closed handler failed.", ex);
                    }
                }
            }
        }
    }
}