using NRpc.Container;
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
            var rpcServer = new NRpcServer(12345);
            rpcServer.Start();
            Console.ReadLine();
            rpcServer.ShutDown();
        }
    }
}