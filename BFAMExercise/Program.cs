using BFAMExercise.Quotation;
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
        private static readonly char _delimiter = ' ';

        static void Main(string[] args)
        {
            var server = new Server.TCPServer("127.0.0.1", 3000);

            // The handler will run in this own threadpool.
            // Don't register async function to this handler.
            server.RequestHandler = (clientMsg, send) =>
            {
                try
                {
                    IBasicQuoteRequestMessageParser parser = new DelimitedBasicQuoteRequestMessageParser(_delimiter);
                    var quoteRequestMsg = parser.Parse(clientMsg);

                    IBasicQuotation basicQuotation = new BasicQuotation(
                        new PriceSource.RandomReferencePriceSource(),
                        new QuoteEngine.ProdAQuoteCalculationEngine());
                    double quote = basicQuotation.GetQuote(quoteRequestMsg);

                    string reponse = clientMsg + _delimiter + quote;
                    send(reponse);
                    Console.WriteLine("Message sent: " + reponse);
                }
                catch (ParseQuoteRequestMessageException ex)
                {
                    Console.WriteLine(String.Format("Cannot parse {0}:\n{1}", clientMsg, ex.ToString()));
                    send("Error");
                }
                catch (BasicQuotationException ex)
                {
                    Console.WriteLine(String.Format("Cannot get quotation for {0}:\n{1}", clientMsg, ex.ToString()));
                    send("Error");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    send("Error");
                }
            };

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
