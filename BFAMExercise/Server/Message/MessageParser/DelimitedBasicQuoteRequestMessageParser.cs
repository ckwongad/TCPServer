using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Server.Message.MessageParser
{
    public class DelimitedBasicQuoteRequestMessageParser : IBasicQuoteRequestMessageParser
    {
        private static readonly int numComponents = 3;

        private readonly char _delimiter;

        public DelimitedBasicQuoteRequestMessageParser(char delimiter = ' ')
        {
            this._delimiter = delimiter;
        }

        public BasicQuoteRequestMessage Parse(string msg)
        {
            var quoteRequestMsg = new BasicQuoteRequestMessage();

            try
            {
                if (msg == null) throw new ArgumentNullException("msg");

                string[] msgComponents = msg.Split(this._delimiter);

                if (msgComponents.Length != numComponents)
                {
                    throw new FormatException(
                        String.Format("Number of message components is {0} instead of {1}. {2}", msgComponents.Length, numComponents, msg));
                }

                quoteRequestMsg.securityId = int.Parse(msgComponents[0]);
                quoteRequestMsg.isBuy = DetermineBuy(msgComponents[1]);
                quoteRequestMsg.quantity = int.Parse(msgComponents[2]);
            } catch (Exception ex) {
                throw new ParseQuoteRequestMessageException("Parse Fail", ex);
            }

            return quoteRequestMsg;
        }

        private static bool DetermineBuy(string str)
        {
            if (str == "BUY") return true;
            if (str == "SELL") return false;

            throw new ArgumentException(String.Format("Expect BUY or SELL, but get {0}", str));
        }
    }
}
