using NRpc.Container;
using NRpc.Proxy;
using NRpc.TestCommon;
using System;

namespace NRpc.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ContainerManager.UseAutofacContainer().UseDefault();
            var user = ProxyFactory.Create<IUser>();
            Console.WriteLine(user.SetAget(10));
            Console.WriteLine(user.GetAge(20));
            Console.WriteLine(user.GetAge());
            Console.ReadLine();
        }
    }
}