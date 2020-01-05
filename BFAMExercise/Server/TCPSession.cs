using AsyncAwaitBestPractices;
using BFAMExercise.Quotation;
using BFAMExercise.Server.Message.MessageParser;
using BFAMExercise.Server.MessageStream;
using BFAMExercise.Server.NetworkSocket;
using BFAMExercise.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BFAMExercise.Server
{
    public class TCPSession : IDisposable
    {
        private static UniqueIdGenerator _idGenerator = new UniqueIdGenerator();

        private readonly char _delimiter = ' ';
        private readonly TcpClient _tcpClient;
        private readonly IMessageStream _stream;

        #region Properties
        private volatile bool _isClose = false;
        public bool IsClose
        {
            get { return _isClose; }
            private set { _isClose = value; }
        }

        public long SessionId { get; private set; }
        #endregion Properties

        #region events
        public event EventHandler<long> OnClose;
        #endregion events

        public TCPSession(TcpClient client)
        {
            SessionId = _idGenerator.GetId();
            this._tcpClient = client;
            this._stream = new DelimiterMessageStream(client.GetStream());
        }

        public void StartAsync()
        {
            Poll();
            AcceptRequestAsync();
        }

        public void Start()
        {
            Poll();
            AcceptRequest();
        }

        private async void AcceptRequestAsync()
        {
            string clientMsg = "";
            try
            {
                while (!IsClose)
                {
                    clientMsg = await this._stream.ReadAsync().ConfigureAwait(false);
                    Log("Message Received: " + clientMsg);
                    HandleRequestAsync(clientMsg).SafeFireAndForget(onException: ex => HandleException(ex));
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void AcceptRequest()
        {
            string clientMsg = "";
            try
            {
                while (!IsClose)
                {
                    clientMsg = this._stream.Read();
                    Log("Message Received: " + clientMsg);
                    HandleRequestAsync(clientMsg).SafeFireAndForget(onException: ex => HandleException(ex));
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private async Task HandleRequestAsync(string clientMsg)
        {
            try
            {
                IBasicQuoteRequestMessageParser parser = new DelimitedBasicQuoteRequestMessageParser(this._delimiter);
                var quoteRequestMsg = parser.Parse(clientMsg);

                IBasicQuotation basicQuotation = new BasicQuotation(
                    new PriceSource.RandomReferencePriceSource(),
                    new QuoteEngine.ProdAQuoteCalculationEngine());
                double quote = await basicQuotation.GetQuoteAsync(quoteRequestMsg);

                string reponse = clientMsg + this._delimiter + quote;
                await this._stream.WriteAsync(reponse);
                Log("Message sent: " + reponse);
            }
            catch (ParseQuoteRequestMessageException ex)
            {
                Log(String.Format("Cannot parse {0}:\n{1}", clientMsg, ex.ToString()));
                this._stream.Write("Error");
            }
            catch (BasicQuotationException ex)
            {
                Log(String.Format("Cannot get quotation for {0}:\n{1}", clientMsg, ex.ToString()));
                this._stream.Write("Error");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async void Poll()
        {
            try
            {
                while (!IsClose)
                {
                    await Task.Delay(300).ConfigureAwait(false);
                    if (!IsConnected)
                    {
                        Log("Connection Terminated. To close...");
                        Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log(String.Format("Polling error: {0}", e));
                Close();
            }
        }

        public bool IsConnected
        {
            get
            {
                //return this._tcpClient.Client.IsConnected();
                try
                {
                    var socket = this._tcpClient.Client;
                    if (socket == null || !socket.Connected)
                        return false;

                    /* pear to the documentation on Poll:
					 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
					 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
					 * -or- true if data is available for reading;
					 * -or- true if the connection has been closed, reset, or terminated;
					 * otherwise, returns false
					 */
                    if (!socket.Poll(1, SelectMode.SelectRead))
                        return true;

                    byte[] buff = new byte[1];
                    var clientSentData = socket.Receive(buff, SocketFlags.Peek) != 0;
                    return clientSentData; //False here though Poll() succeeded means we had a disconnect!
                }
                catch (SocketException ex)
                {
                    Log(ex.ToString());
                    return false;
                }
            }
        }

        public void Close()
        {
            if (!IsClose)
            {
                _stream.Close();
                _tcpClient.Close();
                IsClose = true;
                Log(String.Format("Sessoin closed", SessionId));
                OnClose?.Invoke(this, SessionId);
            }
        }

        private void HandleException(Exception ex)
        {
            if (IsClose) return;

            Log(String.Format("Session to close on error: {0}", ex));
            Close();
        }

        private void Log(string msg)
        {
            Console.WriteLine("Session #{0}: " + msg, SessionId);
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
                    Close();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
