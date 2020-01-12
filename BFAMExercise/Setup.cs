using BFAMExercise.Quotation;
using BFAMExercise.RequestHandler;
using BFAMExercise.Server.Message.MessageParser;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BFAMExercise
{
    public class Setup
    {
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        public static Server.TCPServer SetUpServer(string ip, int port, ILogger logger)
        {
            Server.TCPServer server = new Server.TCPServer(ip, port, logger.ForContext<Server.TCPServer>());
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
            return server;
        }
    }
}
