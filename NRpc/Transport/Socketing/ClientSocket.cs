using NRpc.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NRpc.Transport.Socketing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ClientSocket.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 17:40:08
    /// </summary>
    public class ClientSocket
    {
        #region Private Properties

        private EndPoint _serverEndPoint;
        private EndPoint _localEndPoint;
        private Socket _socket;
        private TcpConnection _connection;
        private readonly SocketSetting _setting;
        private readonly IList<IConnectionEventListener> _connectionEventListeners;
        private readonly Action<ITcpConnection, byte[]> _messageArrivedHandler;
        private readonly ManualResetEvent _waitConnectHandle;
        private readonly int _flowControlThreshold;

        #endregion Private Properties

        public bool IsConnected
        {
            get { return _connection != null && _connection.IsConnected; }
        }

        public TcpConnection Connection
        {
            get { return _connection; }
        }

        public ClientSocket(EndPoint serverEndPoint, EndPoint localEndPoint, SocketSetting setting, Action<ITcpConnection, byte[]> messageArrivedHandler)
        {
            Ensure.NotNull(serverEndPoint, "serverEndPoint");
            Ensure.NotNull(setting, "setting");
            Ensure.NotNull(messageArrivedHandler, "messageArrivedHandler");

            _connectionEventListeners = new List<IConnectionEventListener>();

            _serverEndPoint = serverEndPoint;
            _localEndPoint = localEndPoint;
            _setting = setting;
            _messageArrivedHandler = messageArrivedHandler;
            _waitConnectHandle = new ManualResetEvent(false);
            _socket = SocketUtils.CreateSocket(_setting.SendBufferSize, _setting.ReceiveBufferSize);
            _flowControlThreshold = _setting.SendMessageFlowControlThreshold;
        }

        public ClientSocket RegisterConnectionEventListener(IConnectionEventListener listener)
        {
            _connectionEventListeners.Add(listener);
            return this;
        }

        public ClientSocket Start(int waitMilliseconds = 5000)
        {
            var socketArgs = new SocketAsyncEventArgs();
            socketArgs.AcceptSocket = _socket;
            socketArgs.RemoteEndPoint = _serverEndPoint;
            socketArgs.Completed += OnConnectAsyncCompleted;
            if (_localEndPoint != null)
            {
                _socket.Bind(_localEndPoint);
            }

            var firedAsync = _socket.ConnectAsync(socketArgs);
            if (!firedAsync)
            {
                ProcessConnect(socketArgs);
            }

            _waitConnectHandle.WaitOne(waitMilliseconds);

            return this;
        }

        public ClientSocket QueueMessage(byte[] message)
        {
            _connection.SendMessage(message);
            FlowControlIfNecessary();
            return this;
        }

        public ClientSocket Shutdown()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
            else
            {
                SocketUtils.ShutdownSocket(_socket);
                _socket = null;
            }
            return this;
        }

        private void FlowControlIfNecessary()
        {
            if (_flowControlThreshold > 0 && _connection.PendingMessageCount >= _flowControlThreshold)
            {
                var milliseconds = FlowControlUtil.CalculateFlowControlTimeMilliseconds(
                    (int)_connection.PendingMessageCount,
                    _flowControlThreshold,
                    _setting.SendMessageFlowControlStepPercent,
                    _setting.SendMessageFlowControlWaitMilliseconds);
                Thread.Sleep(milliseconds);
            }
        }

        private void OnConnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            e.Completed -= OnConnectAsyncCompleted;
            e.AcceptSocket = null;
            e.RemoteEndPoint = null;
            e.Dispose();

            if (e.SocketError != SocketError.Success)
            {
                SocketUtils.ShutdownSocket(_socket);
                LogUtil.Info(string.Format("Socket connect failed, socketError:{0}", e.SocketError));
                OnConnectionFailed(e.SocketError);
                _waitConnectHandle.Set();
                return;
            }

            _connection = new TcpConnection(_socket, _setting, OnMessageArrived, OnConnectionClosed);

            LogUtil.Info(string.Format("Socket connected, remote endpoint:{0}, local endpoint:{1}", _connection.RemotingEndPoint, _connection.LocalEndPoint));
            OnConnectionEstablished(_connection);

            _waitConnectHandle.Set();
        }

        private void OnMessageArrived(ITcpConnection connection, byte[] message)
        {
            try
            {
                _messageArrivedHandler(connection, message);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Handle message error.", ex);
            }
        }

        private void OnConnectionEstablished(ITcpConnection connection)
        {
            foreach (var listener in _connectionEventListeners)
            {
                try
                {
                    listener.OnConnectionEstablished(connection);
                }
                catch (Exception ex)
                {
                    LogUtil.Error(string.Format("Notify connection established failed, listener type:{0}", listener.GetType().Name), ex);
                }
            }
        }

        private void OnConnectionFailed(SocketError socketError)
        {
            foreach (var listener in _connectionEventListeners)
            {
                try
                {
                    listener.OnConnectionFailed(socketError);
                }
                catch (Exception ex)
                {
                    LogUtil.Error(string.Format("Notify connection failed has exception, listener type:{0}", listener.GetType().Name), ex);
                }
            }
        }

        private void OnConnectionClosed(ITcpConnection connection, SocketError socketError)
        {
            foreach (var listener in _connectionEventListeners)
            {
                try
                {
                    listener.OnConnectionClosed(connection, socketError);
                }
                catch (Exception ex)
                {
                    LogUtil.Error(string.Format("Notify connection closed failed, listener type:{0}", listener.GetType().Name), ex);
                }
            }
        }
    }
}