using NRpc.Container;
using NRpc.Transport.Remoting;
using NRpc.Utils;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest.Server
{
    internal class Program
    {
        private static SocketRemotingServer _remotingServer;

        private static void Main(string[] args)
        {
            ContainerManager.UseAutofacContainer().UseDefault();
            _remotingServer = new SocketRemotingServer().Start();
            Console.WriteLine("click to sen.");
            Console.ReadLine();
            PushTestMessageToAllClients();
            Console.ReadLine();
        }

        private static void PushTestMessageToAllClients()
        {
            var messageCount = 100;

            Task.Factory.StartNew(() =>
            {
                for (var i = 1; i <= messageCount; i++)
                {
                    try
                    {
                        var remotingServerMessage = new RemotingServerMessage(RemotingServerMessageType.ServerMessage, 100, Encoding.UTF8.GetBytes("message:" + i));
                        _remotingServer.PushMessageToAllConnections(remotingServerMessage);
                        LogUtil.InfoFormat("Pushed server message: {0}", "message:" + i);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.ErrorFormat("PushMessageToAllConnections failed, errorMsg: {0}", ex.Message);
                        Thread.Sleep(1000);
                    }
                }
            });
        }
    }
}