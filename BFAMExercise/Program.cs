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
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

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
            var logger = Log.ForContext<Program>(); ;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                logger.Fatal(e.ExceptionObject.ToString());
                Log.CloseAndFlush();
                throw e.ExceptionObject as Exception;
            };

            Server.TCPServer server = null;
            try
            {
                server = new Server.TCPServer("127.0.0.1", 3000, logger.ForContext<Server.TCPServer>());

                var requestHanlder = new StdRequestHanlder(
                    new DelimitedBasicQuoteRequestMessageParser(' '),
                    new BasicQuotation(
                        new PriceSource.RandomReferencePriceSource(),
                        new QuoteEngine.ProdAQuoteCalculationEngine()
                    ),
                    logger);
                server.RegisterRequestHandler(requestHanlder.ProcessRequest);

                long lastClientTime = 0;
                server.OnNewConnection += ((sender, client) =>
                {
                    var currentClientTime = _sw.ElapsedMilliseconds;
                    var interConTime = lastClientTime == 0 ? 0 : currentClientTime - lastClientTime;
                    logger.Information("Connected! Time passed since last connection: {0}ms", interConTime);
                    lastClientTime = currentClientTime;
                });

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
