using BFAMExercise.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BFAMExercise.Test.Util
{
    [TestClass]
    public class ByteArrayUtilTests
    {
        [TestMethod]
        public void Slice_Null_ThrowArgumentNullException()
        {
            byte[] msgByte = null;
            byte separator = Convert.ToByte('\n');
            Assert.ThrowsException<ArgumentNullException>(() => ByteArrayUtil.Slice(msgByte, separator).ToArray<byte[]>());
        }

        [TestMethod]
        public void Slice_Blank_ReturnEmpty()
        {
            string msg = "";
            byte[] msgByte = System.Text.Encoding.ASCII.GetBytes(msg);
            byte separator = Convert.ToByte('\n');

            Assert.AreEqual(ByteArrayUtil.Slice(msgByte, separator).Count(), 0);
        }

        [TestMethod]
        public void Slice_Separator_ReturnSeparator()
        {
            string msg = "\n";
            byte[] msgByte = System.Text.Encoding.ASCII.GetBytes(msg);
            byte separator = Convert.ToByte('\n');

            foreach (byte[] separatedByte in ByteArrayUtil.Slice(msgByte, separator))
            {
                Assert.IsTrue(separatedByte.SequenceEqual(msgByte));
            }
        }

        [TestMethod]
        public void Slice_MultipleSeparator_ReturnMultipleSeparator()
        {
            string msg = "\n\n\n\n";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes("\n"),
                System.Text.Encoding.ASCII.GetBytes("\n"),
                System.Text.Encoding.ASCII.GetBytes("\n"),
                System.Text.Encoding.ASCII.GetBytes("\n")
            };

            TestEqual(msg, expectations);
        }

        [TestMethod]
        public void Slice_SeparatorFollowedByMsg_ReturnSeparatorFollowedByMsg()
        {
            string msg = "\n1st msg\n";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes("\n"),
                System.Text.Encoding.ASCII.GetBytes("1st msg\n")
            };

            TestEqual(msg, expectations);
        }

        [TestMethod]
        public void Slice_IncompleteItem_ReturnIncompleteItem()
        {
            string msg = "1st msg";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes(msg)
            };

            TestEqual(msg, expectations);
        }

        [TestMethod]
        public void Slice_SingleItem_ReturnSingleItem()
        {
            string msg = "1st msg\n";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes(msg)
            };

            TestEqual(msg, expectations);
        }

        [TestMethod]
        public void Slice_IncompleteDoubleItem_ReturnIncompleteDoubleItem()
        {
            string msg = "1st msg\n2nd msg";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes("1st msg\n"),
                System.Text.Encoding.ASCII.GetBytes("2nd msg")
            };

            TestEqual(msg, expectations);
        }

        [TestMethod]
        public void Slice_DoubleItem_ReturnDoubleItem()
        {
            string msg = "1st msg\n2nd msg\n";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes("1st msg\n"),
                System.Text.Encoding.ASCII.GetBytes("2nd msg\n")
            };

            TestEqual(msg, expectations);
        }

        [TestMethod]
        public void Slice_NonstandardSeparator_ReturnSeparated()
        {
            string msg = "1st msg&2nd msg&";
            byte[][] expectations = new byte[][]
            {
                System.Text.Encoding.ASCII.GetBytes("1st msg&"),
                System.Text.Encoding.ASCII.GetBytes("2nd msg&")
            };

            TestEqual(msg, expectations, Convert.ToByte('&'));
        }

        private static void TestEqual(string msg, byte[][] expectations, byte separator = 10)
        {
            // byte 10 is \n
            byte[] msgByte = System.Text.Encoding.ASCII.GetBytes(msg);

            var separatedBytes = ByteArrayUtil.Slice(msgByte, separator).ToArray();
            for (int i = 0; i < separatedBytes.Count(); i++)
            {
                Assert.IsTrue(separatedBytes[i].SequenceEqual(expectations[i]));
            }
        }
    }
}
