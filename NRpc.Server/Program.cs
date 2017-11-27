using NRpc.Container;
using NRpc.Proxy;
using NRpc.TestCommon;
using System;

namespace NRpc.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ContainerManager.UseAutofacContainer().UseDefault()
                .AddScoped<IUser, User>();
            var proxyServer = new ProxyServer(12345);
            proxyServer.Start();
            Console.ReadLine();
            proxyServer.ShutDown();
        }
    }
}