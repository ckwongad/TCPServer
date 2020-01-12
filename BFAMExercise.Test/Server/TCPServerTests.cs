using BFAMExercise.Quotation;
using BFAMExercise.RequestHandler;
using BFAMExercise.Server;
using BFAMExercise.Server.Message.MessageParser;
using BFAMExercise.Server.MessageStream;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExercise.Test.Server
{
    [TestClass]
    public class TCPServerTests
    {
        private TCPServer _server;
        private Thread _thread;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private DelimiterMessageStream _msgStream;

        [TestInitialize]
        public void TestInitialize()
        {
            StartServer();

            _tcpClient = new TcpClient("127.0.0.1", 3000);
            _stream = _tcpClient.GetStream();
            _msgStream = new DelimiterMessageStream(_tcpClient.GetStream());
        }

        private void StartServer()
        {
            _server = Setup.SetUpServer("127.0.0.1", 3000, Log.Logger);
            _server.ListenAsync();
            //_thread = new Thread(_server.Listen);
            //_thread.Start();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _stream.Close();
            _msgStream.Close();
            _tcpClient.Close();
            _server.Stop();
        }

        [TestMethod]
        public async Task BeesWithGuns_Success()
        {
            var result = await BFAMExerciseClient.BeeHive.AttackAsync(100, 5);
            Assert.IsTrue(result);
            Assert.IsFalse(_server.IsStop);
        }

        [TestMethod]
        public void Listen_Success()
        {
            string message = "123 BUY 100";

            int count = 0;
            while (count++ < 3)
            {
                _msgStream.Write(message);
                Console.WriteLine("Sent: {0}", message);

                var response = _msgStream.Read();
                Console.WriteLine("Received: {0}", response);
                Assert.AreEqual(message, response.Substring(0, message.Length));
            }
        }

        [TestMethod]
        public void Listen_IncompleteMsg_CombineIncompleteMsg()
        {
            WriteToStream("123 BUY 100\n345 SELL");
            string response = _msgStream.Read();
            var expectation = "123 BUY 100";
            Assert.AreEqual(expectation, response.Substring(0, expectation.Length));

            Thread.Sleep(200);

            WriteToStream(" 100\n");
            response = _msgStream.Read();
            expectation = "345 SELL 100";
            Assert.AreEqual(expectation, response.Substring(0, expectation.Length));
        }

        private void WriteToStream(string message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }
    }
}
