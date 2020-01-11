using BFAMExercise.Quotation;
using BFAMExercise.RequestHandler;
using BFAMExercise.Server.Message.MessageParser;
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
            var server = new Server.TCPServer("127.0.0.1", 3000);

            var requestHanlder = new StdRequestHanlder(
                new DelimitedBasicQuoteRequestMessageParser(' '),
                new BasicQuotation(
                    new PriceSource.RandomReferencePriceSource(),
                    new QuoteEngine.ProdAQuoteCalculationEngine()
                ));
            server.RegisterRequestHandler(requestHanlder.ProcessRequest);

            long lastClientTime = 0;
            server.OnNewConnection += ((sender, client) =>
            {
                var currentClientTime = _sw.ElapsedMilliseconds;
                var interConTime = lastClientTime == 0 ? 0 : currentClientTime - lastClientTime;
                Console.WriteLine("Connected! Time passed since last connection: {0}ms", interConTime);
                if (interConTime > 2000) throw new Exception("interConTime > 2000");
                lastClientTime = currentClientTime;
            });

            server.ListenAsync();

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
