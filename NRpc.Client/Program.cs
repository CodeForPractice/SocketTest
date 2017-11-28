using NRpc.Container;
using NRpc.TestCommon;
using System;
using System.Threading.Tasks;

namespace NRpc.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ContainerManager.UseAutofacContainer().UseDefault();
            ExecuteAsync().Wait();
            Console.WriteLine("==========");
            Console.ReadLine();
        }

        private static async Task ExecuteAsync()
        {
            var user = ProxyFactory.Create<IUser>();
            await user.AsyncAction();
            Console.WriteLine("1执行完成");
            var age = await user.GetAgeAsync();
            user.SetMessage();
            Console.WriteLine(age);
            Console.WriteLine(user.SetAget(10));
            Console.WriteLine(user.GetAge(20));
            Console.WriteLine(user.GetAge());
            Console.WriteLine(user.Model()?.ToString());
            var nullModel = user.GetNull();
            var nullAsync = await user.GetNullAsync();
            if (nullAsync == null)
            {
                Console.WriteLine("empty");
            }
            else
            {
                Console.WriteLine(nullAsync.ToString());
            }
        }
    }
}