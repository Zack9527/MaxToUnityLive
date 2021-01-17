using System;
using MaxServer_dll;

namespace MaxServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
            Console.ReadLine();
            server.Stop();
            Console.ReadLine();
        }
    }
}
