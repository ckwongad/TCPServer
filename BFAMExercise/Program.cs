using BFAMExercise.Quotation;
using BFAMExercise.RequestHandler;
using BFAMExercise.Server.Message.MessageParser;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExercise
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => {
                    a.File("log.txt", rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] [Session: {SessionId}] {Message:lj}{NewLine}{Exception}",
                        buffered: true);
                    a.Console();
                })
                .CreateLogger();
            var logger = Log.ForContext<Program>();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                logger.Fatal(e.ExceptionObject.ToString());
                Log.CloseAndFlush();
                throw e.ExceptionObject as Exception;
            };

            Server.TCPServer server = null;
            try
            {
                server = Setup.SetUpServer("127.0.0.1", 3000, logger);
                server.ListenAsync();
                Console.Read();
            }
            finally
            {
                server?.Stop();
                Log.CloseAndFlush();
            }
        }
    }
}
