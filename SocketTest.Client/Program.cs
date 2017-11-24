using NRpc.Container;
using NRpc.Transport.Remoting;
using NRpc.Transport.Socketing;
using NRpc.Utils;
using System;
using System.Net;
using System.Text;

namespace SocketTest.Client
{
    internal class Program
    {
        private static SocketRemotingClient _client;

        private static void Main(string[] args)
        {
            ContainerManager.UseAutofacContainer().UseDefault();
            var serverAddress = SocketUtils.GetLocalIPV4();
            _client = new SocketRemotingClient(new IPEndPoint(serverAddress, 5000)).Start();
            _client.RegisterRemotingServerMessageHandler(100, new RemotingServerMessageHandler());
            Console.ReadKey();
        }

        private class RemotingServerMessageHandler : IRemotingServerMessageHandler
        {
            public void HandleMessage(RemotingServerMessage message)
            {
                if (message.Code != 100)
                {
                    LogUtil.ErrorFormat("Invalid remoting server message: {0}", message);
                    return;
                }
                LogUtil.InfoFormat("Received server message: {0}", Encoding.UTF8.GetString(message.Body));
            }
        }
    }
}