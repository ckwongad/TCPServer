using BFAMExercise.Server;
using BFAMExercise.Server.MessageStream;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            _server = new TCPServer("127.0.0.1", 3000);
            _thread = new Thread(_server.Listen);
            _thread.Start();
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
        public void Bees_with_Gun()
        {
            BFAMExerciseClient.BeesWithGuns.Attack(100, 5);
        }

        [TestMethod]
        public void Listen_StateUnderTest_ExpectedBehavior()
        {
            string message = "123 BUY 100\n";

            int count = 0;
            while (count++ < 3)
            {
                // Translate the Message into ASCII.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Send the message to the connected TcpServer. 
                _stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", message);

                // Bytes Array to receive Server Response.
                data = new Byte[256];
                String response = String.Empty;

                // Read the Tcp Server Response Bytes.
                Int32 bytes = _stream.Read(data, 0, data.Length);
                response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", response);

                Thread.Sleep(200);
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
