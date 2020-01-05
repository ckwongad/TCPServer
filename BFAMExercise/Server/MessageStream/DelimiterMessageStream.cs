using BFAMExercise.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BFAMExercise.Server.MessageStream
{
    public class DelimiterMessageStream : IMessageStream
    {
        private readonly Stream _stream;
        private readonly Queue<string> _messageBuffer = new Queue<string>();
        private byte[] _unprocessedBytes = new byte[0];

        private readonly object _readStreamLock = new object();
        private readonly object _writeStreamLock = new object();

        public int MaxLength { get; set; } = 256;
        public byte Separator { get; set; } = 10; // \n

        public DelimiterMessageStream(Stream stream)
        {
            this._stream = stream;
        }

        public async Task<string> ReadAsync()
        {
            byte[] streamBytes = new byte[this.MaxLength];

            while (this._messageBuffer.Count == 0)
            {
                var i = await this._stream.ReadAsync(streamBytes, 0, streamBytes.Length);
                byte[] dataByte = BuildDataBytes(streamBytes, i);

                foreach (var slicedBytes in ByteArrayUtil.Slice(dataByte, this.Separator))
                {
                    if (slicedBytes[slicedBytes.Length - 1] == this.Separator)
                    {
                        this._messageBuffer.Enqueue(Encoding.ASCII.GetString(slicedBytes, 0, slicedBytes.Length - 1));
                    }
                    else
                    {
                        if (this._unprocessedBytes.Length > 0)
                            throw new Exception("Length of unprocessed byte array should be 0");

                        this._unprocessedBytes = slicedBytes;
                    }
                }

                if (this._unprocessedBytes.Length >= this.MaxLength)
                {
                    throw new MessageTooLongException(
                        String.Format("Message is longer than {0}", this.MaxLength)
                    );
                }
            }

            return this._messageBuffer.Dequeue();
        }

        public string Read()
        {
            lock (this._readStreamLock)
            {
                byte[] streamBytes = new byte[this.MaxLength];

                while (this._messageBuffer.Count == 0)
                {
                    var i = this._stream.Read(streamBytes, 0, streamBytes.Length);
                    byte[] dataByte = BuildDataBytes(streamBytes, i);

                    foreach (var slicedBytes in ByteArrayUtil.Slice(dataByte, this.Separator))
                    {
                        if (slicedBytes[slicedBytes.Length - 1] == this.Separator)
                        {
                            this._messageBuffer.Enqueue(Encoding.ASCII.GetString(slicedBytes, 0, slicedBytes.Length - 1));
                        }
                        else
                        {
                            if (this._unprocessedBytes.Length > 0)
                                throw new Exception("Length of unprocessed byte array should be 0");

                            this._unprocessedBytes = slicedBytes;
                        }
                    }

                    if (this._unprocessedBytes.Length >= this.MaxLength)
                    {
                        throw new MessageTooLongException(
                            String.Format("Message is longer than {0}", this.MaxLength)
                        );
                    }
                }

                return this._messageBuffer.Dequeue();
            }
        }

        //
        // Summary:
        //     Merge unprocessed byte array and byte array from stream to form data byte array
        private byte[] BuildDataBytes(byte[] streamBytes, int i)
        {
            byte[] dataByte = new byte[this._unprocessedBytes.Length + i];
            Array.Copy(this._unprocessedBytes, 0, dataByte, 0, this._unprocessedBytes.Length);
            Array.Copy(streamBytes, 0, dataByte, this._unprocessedBytes.Length, i);
            this._unprocessedBytes = new byte[0];
            return dataByte;
        }

        public async Task WriteAsync(string msg)
        {
            byte[] msgByte = System.Text.Encoding.ASCII.GetBytes(msg);
            await this._stream.WriteAsync(msgByte, 0, msgByte.Length);
            this._stream.WriteByte(Separator);
        }

        public void Write(string msg)
        {
            lock (this._writeStreamLock)
            {
                byte[] msgByte = System.Text.Encoding.ASCII.GetBytes(msg);
                this._stream.Write(msgByte, 0, msgByte.Length);
                this._stream.WriteByte(Separator);
            }
        }

        public void Close()
        {
            this._stream.Close();
        }

        #region IDisposable implementation

        // Disposed flag.
        private bool _disposed;

        // Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedResources)
        {
            if (!_disposed)
            {
                if (disposingManagedResources)
                {
                    this._stream.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }

    [Serializable]
    public class MessageTooLongException : Exception
    {
        public MessageTooLongException() { }
        public MessageTooLongException(string message) : base(message) { }
        public MessageTooLongException(string message, Exception inner) : base(message, inner) { }
        protected MessageTooLongException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
