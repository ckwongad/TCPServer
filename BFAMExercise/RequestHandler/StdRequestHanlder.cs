using BFAMExercise.Quotation;
using BFAMExercise.Server.Message.MessageParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BFAMExercise.RequestHandler
{
    class StdRequestHanlder : IRequestHanlder
    {
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private IBasicQuoteRequestMessageParser _messageParser;
        private IBasicQuotation _basicQuotation;

        public StdRequestHanlder(IBasicQuoteRequestMessageParser parser, IBasicQuotation basicQuotation)
        {
            _messageParser = parser;
            _basicQuotation = basicQuotation;
        }

        public void ProcessRequest(string clientMsg, Action<string> send)
        {
            try
            {
                var receiveTime = _sw.ElapsedMilliseconds;
                var quoteRequestMsg = _messageParser.Parse(clientMsg);

                double quote = _basicQuotation.GetQuote(quoteRequestMsg);

                string response = clientMsg + ' ' + quote;
                send(response);
                Console.WriteLine("Response: {0}. Process Time: {1}", response, _sw.ElapsedMilliseconds - receiveTime);
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
        }
    }
}
