using NRpc.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NRpc.Transport.Socketing
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ServerSocket.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 17:48:33
    /// </summary>
    public class ServerSocket
    {
        #region Private Variables

        private readonly Socket _socket;
        private readonly SocketSetting _setting;
        private readonly IPEndPoint _listeningEndPoint;
        private readonly SocketAsyncEventArgs _acceptSocketArgs;
        private readonly IList<IConnectionEventListener> _connectionEventListeners;
        private readonly Action<ITcpConnection, byte[], Action<byte[]>> _messageArrivedHandler;

        #endregion Private Variables

        public ServerSocket(IPEndPoint listeningEndPoint, SocketSetting setting, Action<ITcpConnection, byte[], Action<byte[]>> messageArrivedHandler)
        {
            Ensure.NotNull(listeningEndPoint, "listeningEndPoint");
            Ensure.NotNull(setting, "setting");
            Ensure.NotNull(messageArrivedHandler, "messageArrivedHandler");

            _listeningEndPoint = listeningEndPoint;
            _setting = setting;
            _connectionEventListeners = new List<IConnectionEventListener>();
            _messageArrivedHandler = messageArrivedHandler;
            _socket = SocketUtils.CreateSocket(_setting.SendBufferSize, _setting.ReceiveBufferSize);
            _acceptSocketArgs = new SocketAsyncEventArgs();
            _acceptSocketArgs.Completed += AcceptCompleted;
        }

        public void RegisterConnectionEventListener(IConnectionEventListener listener)
        {
            _connectionEventListeners.Add(listener);
        }

        public void Start()
        {
            LogUtil.Info(string.Format("Socket server is starting, listening on TCP endpoint: {0}.", _listeningEndPoint));

            try
            {
                _socket.Bind(_listeningEndPoint);
                _socket.Listen(5000);
            }
            catch (Exception ex)
            {
                LogUtil.Error(string.Format("Failed to listen on TCP endpoint: {0}.", _listeningEndPoint), ex);
                SocketUtils.ShutdownSocket(_socket);
                throw;
            }

            StartAccepting();
        }

        public void Shutdown()
        {
            SocketUtils.ShutdownSocket(_socket);
            LogUtil.Info(string.Format("Socket server shutdown, listening TCP endpoint: {0}.", _listeningEndPoint));
        }

        private void StartAccepting()
        {
            try
            {
                var firedAsync = _socket.AcceptAsync(_acceptSocketArgs);
                if (!firedAsync)
                {
                    ProcessAccept(_acceptSocketArgs);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ObjectDisposedException))
                {
                    LogUtil.Info("Socket accept has exception, try to start accepting one second later." + ex.ToString());
                }
                Thread.Sleep(1000);
                StartAccepting();
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    OnSocketAccepted(acceptSocket);
                }
                else
                {
                    SocketUtils.ShutdownSocket(e.AcceptSocket);
                    e.AcceptSocket = null;
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                LogUtil.Error("Process socket accept has exception.", ex);
            }
            finally
            {
                StartAccepting();
            }
        }

        private void OnSocketAccepted(Socket socket)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var connection = new TcpConnection(socket, _setting, OnMessageArrived, OnConnectionClosed);
                    LogUtil.Info(string.Format("Socket accepted, remote endpoint:{0}", socket.RemoteEndPoint));
                    foreach (var listener in _connectionEventListeners)
                    {
                        try
                        {
                            listener.OnConnectionAccepted(connection);
                        }
                        catch (Exception ex)
                        {
                            LogUtil.Error(string.Format("Notify connection accepted failed, listener type:{0}", listener.GetType().Name), ex);
                        }
                    }
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    LogUtil.Info("Accept socket client has unknown exception." + ex.ToString());
                }
            });
        }

        private void OnMessageArrived(ITcpConnection connection, byte[] message)
        {
            try
            {
                _messageArrivedHandler(connection, message, reply => connection.SendMessage(reply));
            }
            catch (Exception ex)
            {
                LogUtil.Error("Handle message error.", ex);
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