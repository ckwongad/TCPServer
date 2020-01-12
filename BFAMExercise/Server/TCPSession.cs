using AsyncAwaitBestPractices;
using BFAMExercise.Quotation;
using BFAMExercise.Server.Message.MessageParser;
using BFAMExercise.Server.MessageStream;
using BFAMExercise.Server.NetworkSocket;
using BFAMExercise.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExercise.Server
{
    public class TCPSession : IDisposable
    {
        private static UniqueIdGenerator _idGenerator = new UniqueIdGenerator();

        private readonly TcpClient _tcpClient;
        private readonly IMessageStream _stream;
        private readonly ILogger _logger;

        private readonly Object _thisLock = new object();

        #region Properties
        private volatile int _isClose = 0;
        public bool IsClose
        {
            get { return _isClose == 1; }
            private set { _isClose = value ? 1 : 0; }
        }

        private volatile int _isActive = 0;
        public bool IsActive
        {
            get { return _isActive == 1; }
            private set { _isActive = value ? 1 : 0; }
        }

        public long SessionId { get; private set; }
        #endregion Properties

        #region events
        public event EventHandler<long> OnClose;
        public event EventHandler<string> OnMsg;
        #endregion events

        public TCPSession(TcpClient client, ILogger logger)
        {
            SessionId = _idGenerator.GetId();
            _tcpClient = client;
            _stream = new DelimiterMessageStream(client.GetStream());
            _logger = logger.ForContext<TCPSession>().ForContext("SessionId", SessionId);
        }

        public void StartAsync()
        {
            if (0 == Interlocked.CompareExchange(ref _isActive, 1, 0))
            {
                Poll();
                AcceptRequestAsync();
            }
        }

        public void Start()
        {
            if (0 == Interlocked.CompareExchange(ref _isActive, 1, 0))
            {
                Poll();
                AcceptRequest();
            }
        }

        private async void AcceptRequestAsync()
        {
            string clientMsg = "";
            try
            {
                while (!IsClose)
                {
                    clientMsg = await this._stream.ReadAsync().ConfigureAwait(false);
                    _logger.Verbose("Message Received: {ClientMsg}", clientMsg);
                    OnMsg?.Invoke(this, clientMsg);
                }
            }
            catch (Exception e)
            {
                LogErrorAndClose(e);
            }
        }

        private void AcceptRequest()
        {
            string clientMsg = "";
            try
            {
                while (!IsClose)
                {
                    clientMsg = _stream.Read();
                    _logger.Verbose("Message Received: {ClientMsg}", clientMsg);
                    OnMsg?.Invoke(this, clientMsg);
                }
            }
            catch (Exception e)
            {
                LogErrorAndClose(e);
            }
        }

        public void Write(string msg)
        {
            _stream.Write(msg);
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
                        _logger.Information("Connection Terminated. Session will close.");
                        Close();
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error("Polling error: {Exception}", exception);
                LogErrorAndClose(exception);
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
                    _logger.Error(ex.ToString());
                    return false;
                }
            }
        }

        public void Close()
        {
            if (0 == Interlocked.CompareExchange(ref _isClose, 1, 0))
            {
                IsActive = false;
                _stream.Close();
                _tcpClient.Close();
                _logger.Information("Sessoin closed");
                OnClose?.Invoke(this, SessionId);
            }
        }

        private void LogErrorAndClose(Exception error)
        {
            if (IsClose) return;

            _logger.Error("Session to close on error. {Error}", error);
            Close();
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

        ~TCPSession() => Dispose(false);
    }
}
