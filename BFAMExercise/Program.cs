using System;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExercise
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server.TCPServer("127.0.0.1", 3000);
            server.Listen();

            new Thread(() =>
            {
                while (!server.IsStop)
                {
                    server.WriteReport();
                    Thread.Sleep(1000);
                }
            }).Start();

            Console.Read();
        }
    }
}
