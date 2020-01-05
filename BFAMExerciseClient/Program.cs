using BFAMExercise.Server.MessageStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExerciseClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            sw.Start();
            var startTime = sw.ElapsedMilliseconds;

            BeesWithGuns.Attack(50, 5);

            Console.WriteLine("Completion Time: {0}.", sw.ElapsedMilliseconds - startTime);
            Console.Read();
        }
    }
}
