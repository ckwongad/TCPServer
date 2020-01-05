using BFAMExercise.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;

namespace BFAMExercise.Test.Server
{
    [TestClass]
    public class TCPSessionTests
    {
        private MemoryStream mockStream;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockStream = new MemoryStream();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // TODO
        }

        private TCPSession CreateTCPSession()
        {
            return new TCPSession(mockStream);
        }

        [TestMethod]
        public void Start_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var tCPSession = this.CreateTCPSession();

            // Act
            Thread t = new Thread(tCPSession.Start);
            t.Start();

            string str = "Test!";
            Byte[] reply = System.Text.Encoding.ASCII.GetBytes(str);
            mockStream.Write(reply);
            // mockStream.Flush();
            mockStream.Position = 0;

            var l = mockStream.Length;

            Byte[] bytes = new Byte[5];
            var i = mockStream.Read(bytes, 0, bytes.Length);
            string data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

            // Assert
            Assert.Fail();
        }

        [TestMethod]
        public void Dispose_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var tCPSession = this.CreateTCPSession();

            // Act
            tCPSession.Dispose();

            // Assert
            Assert.Fail();
        }
    }
}
