using BFAMExercise.Server.MessageStream;
using Serilog;
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
            while (true)
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Async(a =>
                    {
                        a.File("log.txt", rollingInterval: RollingInterval.Day, buffered: true);
                        a.Console();
                    })
                    .CreateLogger();

                var sw = Stopwatch.StartNew();
                sw.Start();
                var startTime = sw.ElapsedMilliseconds;

                BeesWithGuns.Attack(100, 5);

                Log.Logger.Information("Completion Time: {0}.", sw.ElapsedMilliseconds - startTime);
                Log.CloseAndFlush();
                Console.Read();
            }
        }
    }
}
