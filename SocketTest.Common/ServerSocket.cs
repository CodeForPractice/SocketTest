using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ServerSocket.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/22 10:51:47
    /// </summary>
    public class ServerSocket
    {
        private Socket _socket;
        private ManualResetEvent _manualResetEvent;
        private SocketAsyncEventArgs _acceptArgs;
        private List<TcpConnetion> list = new List<TcpConnetion>();

        private int _receivedMessageCount = 0;

        public ServerSocket()
        {
            _socket = SocketUtil.Create(10 * 1024, 40 * 1024);
            _manualResetEvent = new ManualResetEvent(false);
            _acceptArgs = new SocketAsyncEventArgs();
            _acceptArgs.Completed += AcceptEventArg_Completed;
            //_acceptArgs.AcceptSocket = _socket;
        }

        public ServerSocket Start(int port)
        {
            var ipEndPoint = new IPEndPoint(SocketUtil.GetLocalIPV4(), port);
            LogUtil.Info(ipEndPoint.ToString());
            _socket.Bind(ipEndPoint);
            _socket.Listen(15);
            StartAccept();
            return this;
        }

        private void StartAccept()
        {
            try
            {
                var willRaiseEvent = _socket.AcceptAsync(_acceptArgs);
                if (!willRaiseEvent)
                {
                    ProcessAccept(_acceptArgs);
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
                StartAccept();
            }
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                LogUtil.Info("Accepted");

                if (e.SocketError != SocketError.Success)
                {
                    e.AcceptSocket.ShutDownCurrent();
                    e.AcceptSocket = null;
                }
                else
                {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    var tcpConnetion = new TcpConnetion(acceptSocket, MessageArrived);
                    LogUtil.Info($"Socket accepted, remote endpoint:{acceptSocket.RemoteEndPoint}");
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
            }
            finally
            {
                StartAccept();
            }
        }

        private void MessageArrived(TcpConnetion tcpConnetion, byte[] messageBytes)
        {
            var currentMessageCount = Interlocked.Increment(ref _receivedMessageCount);
            LogUtil.Fatal($"当前已接收{currentMessageCount.ToString()}条记录");
            LogUtil.Info($"接收来自客户端发送的信息:{tcpConnetion.RemotingEndPoin}:【{Encoding.UTF8.GetString(messageBytes)}】");
            tcpConnetion.SendMessage(Encoding.UTF8.GetBytes($"这是对第{Encoding.UTF8.GetString(messageBytes)}信息的回复"));
        }

        public void Clsoe()
        {
            _socket.ShutDownCurrent();
        }
    }
}