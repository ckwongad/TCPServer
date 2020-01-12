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
        private readonly Serilog.ILogger _logger;
        private IBasicQuoteRequestMessageParser _messageParser;
        private IBasicQuotation _basicQuotation;

        public StdRequestHanlder(
            IBasicQuoteRequestMessageParser parser,
            IBasicQuotation basicQuotation,
            Serilog.ILogger logger)
        {
            _messageParser = parser;
            _basicQuotation = basicQuotation;
            _logger = logger.ForContext<StdRequestHanlder>();
        }

        public void ProcessRequest(string clientMsg, Action<string> send)
        {
            try
            {
                _logger.Information("Message received: {0}", clientMsg);
                var receiveTime = _sw.ElapsedMilliseconds;
                var quoteRequestMsg = _messageParser.Parse(clientMsg);

                double quote = _basicQuotation.GetQuote(quoteRequestMsg);

                string response = clientMsg + ' ' + quote;
                send(response);
                _logger.Information("Response: {0}. Process Time: {1}", response, _sw.ElapsedMilliseconds - receiveTime);
            }
            catch (ParseQuoteRequestMessageException ex)
            {
                _logger.Error(String.Format("Cannot parse {0}:\n{1}", clientMsg, ex.ToString()));
                send("Error");
            }
            catch (BasicQuotationException ex)
            {
                _logger.Error(String.Format("Cannot get quotation for {0}:\n{1}", clientMsg, ex.ToString()));
                send("Error");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                send("Error");
            }
        }
    }
}
