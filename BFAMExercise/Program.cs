using System;

namespace BFAMExercise
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server.TCPServer("127.0.0.1", 3000).Listen();
        }
    }
}
