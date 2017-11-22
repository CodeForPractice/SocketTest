using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketTest.Common;

namespace SocketTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ServerSocket();
            server.Start(6789);
            Console.ReadLine();
            server.Clsoe();
        }
    }
}
