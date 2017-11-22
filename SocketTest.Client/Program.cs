using SocketTest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientSocket = new ClientSocket();
            clientSocket.Start("192.168.129.194", 6789);
            Parallel.For(0, 100, i =>
            {
                clientSocket.SendMessage(Encoding.UTF8.GetBytes(i.ToString()));
            });

            Console.ReadKey();
        }
    }
}
