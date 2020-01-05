using BFAMExercise.Server.Message.MessageParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BFAMExercise.Test.Server.Message.MessageParser
{
    [TestClass]
    public class DelimitedBasicQuoteRequestMessageParserTests
    {
        private static readonly DelimitedBasicQuoteRequestMessageParser _parser
            = new DelimitedBasicQuoteRequestMessageParser();

        [TestMethod]
        public void Parse_BuyMessage_Success()
        {
            var msg = _parser.Parse("123 BUY 100");

            Assert.AreEqual(123, msg.securityId);
            Assert.AreEqual(true, msg.isBuy);
            Assert.AreEqual(100, msg.quantity);
        }

        [TestMethod]
        public void Parse_SellMessage_Success()
        {
            var msg = _parser.Parse("123 SELL 100");

            Assert.AreEqual(123, msg.securityId);
            Assert.AreEqual(false, msg.isBuy);
            Assert.AreEqual(100, msg.quantity);
        }

        [TestMethod]
        public void Parse_SlashDelimitedMsg_Success()
        {
            var parser = new DelimitedBasicQuoteRequestMessageParser('&');
            var msg = parser.Parse("123&SELL&100");

            Assert.AreEqual(123, msg.securityId);
            Assert.AreEqual(false, msg.isBuy);
            Assert.AreEqual(100, msg.quantity);
        }

        [TestMethod]
        public void Parse_InvalidSecurityId_Throw()
        {
            Assert.ThrowsException<ParseQuoteRequestMessageException>(() => {
                _parser.Parse("123.1 BUY 100");
            });
        }

        [TestMethod]
        public void Parse_InvalidQuantity_Throw()
        {
            Assert.ThrowsException<ParseQuoteRequestMessageException>(() => {
                _parser.Parse("123 BUY 100.1");
            });
        }

        [TestMethod]
        public void Parse_InvalidBuy_Throw()
        {
            Assert.ThrowsException<ParseQuoteRequestMessageException>(() => {
                _parser.Parse("123 Buy 100.1");
            });
        }

        [TestMethod]
        public void Parse_IncompleteMsg_Throw()
        {
            Assert.ThrowsException<ParseQuoteRequestMessageException>(() => {
                _parser.Parse("123 Buy");
            });
        }
    }
}
