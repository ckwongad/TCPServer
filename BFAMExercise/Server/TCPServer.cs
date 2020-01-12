using Amib.Threading;
using AsyncAwaitBestPractices;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BFAMExercise.Server
{
    public class TCPServer
    {
        private static readonly Lazy<Logger> _defaultLogger =
            new Lazy<Logger> (() =>
                new LoggerConfiguration()
                  .WriteTo.Async(a => a.File("serverlog.txt", rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] [Session: {SessionId}] {Message:lj}{NewLine}{Exception}",
                    buffered: true))
                  .CreateLogger()
            );

        private readonly TcpListener server = null;
        private readonly ConcurrentDictionary<long, TCPSession> sessions = new ConcurrentDictionary<long, TCPSession>();
        private readonly SmartThreadPool _smartThreadPool = new SmartThreadPool();
        private readonly ILogger _logger;

        #region Properties
        private volatile bool _isStop = false;
        public bool IsStop
        {
            get { return _isStop; }
            private set { _isStop = value; }
        }
        public int ReportInterval { get; set; } = 1000;
        #endregion Properties

        #region Events
        public event EventHandler<TcpClient> OnNewConnection;
        #endregion Events

        #region Delegates
        private Action<string, Action<string>> _requestHandling;
        #endregion Delegates

        public TCPServer(string ip, int port, ILogger logger)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();

            _smartThreadPool.MaxThreads = 25;

            _logger = logger.ForContext<TCPServer>();

            new Thread(() =>
            {
                while (!IsStop)
                {
                    WriteReport();
                    Thread.Sleep(ReportInterval);
                }
            }).Start();
        }

        public TCPServer(string ip, int port)
            : this(ip,
                  port,
                  _defaultLogger.Value)
        {
        }

        public async void ListenAsync()
        {
            try
            {
                while (true)
                {
                    _logger.Verbose("Waiting for a connection...");
                    TcpClient client = await server.AcceptTcpClientAsync();
                    OnNewConnection?.Invoke(this, client);

                    CreateSession(client);
                }
            }
            catch (SocketException e)
            {
                _logger.Fatal("SocketException: {0}", e);
                Stop();
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception: {0}", e);
                Stop();
            }
        }

        public void Listen()
        {
            try
            {
                while (true)
                {
                    _logger.Verbose("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
                    OnNewConnection?.Invoke(this, client);

                    CreateSession(client);
                }
            }
            catch (SocketException e)
            {
                _logger.Fatal("SocketException: {0}", e);
                Stop();
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception: {0}", e);
                Stop();
            }
        }

        private void CreateSession(TcpClient client)
        {
            try
            {
                var session = new TCPSession(client, _logger);
                if (sessions.TryAdd(session.SessionId, session))
                {
                    session.OnClose += (sender, sessionId) => {
                        if (!sessions.TryRemove(sessionId, out var tmp))
                            _logger.Error("Couldn't remove session with session id: {0}.", sessionId);
                    };
                    session.OnMsg += (sender, msg) => {
                        _smartThreadPool.QueueWorkItem(() => {
                            try
                            {
                                if (_requestHandling == null)
                                {
                                    throw new NotImplementedException("Should register at least one request handler");
                                }
                                else
                                {
                                    _requestHandling.Invoke(msg, session.Write);
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.Error("Error in handling request. {0}", e);
                                session.Close();
                            }
                        });
                    };
                    session.StartAsync();
                    //Thread t = new Thread(session.Start);
                    //t.Start();
                    _logger.Information("Session #{0} started. Total # of sessions: {1}", session.SessionId, sessions.Count);
                }
                else
                {
                    _logger.Error("Couldn't add session with session id: {0}.", session.SessionId);
                    session.Close();
                }
            }
            catch (Exception e)
            {
                _logger.Error("Failed to create session: {0}.", e);
            }
        }

        public void RegisterRequestHandler(Action<string, Action<string>> requestHandler)
        {
            this._requestHandling = requestHandler;
        }

        private void WriteReport()
        {
            var reportTemplate = @"
=====================================================
Worker thread pool of server
# of active threads: {3}
# of threads in use: {4}

.Net thread pool
# of active threads: {0}

Total # of threads: {1}

Total # of sessions: {2}
=====================================================
";

            int maxT, AvailableT, tmp;
            ThreadPool.GetMaxThreads(out maxT, out tmp);
            ThreadPool.GetAvailableThreads(out AvailableT, out tmp);
            int totalThreads = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
            _logger.Information(reportTemplate, maxT - AvailableT,
                totalThreads, sessions.Count,
                _smartThreadPool.InUseThreads, _smartThreadPool.ActiveThreads);

        }

        public void Stop()
        {
            if (IsStop) return;

            server.Stop();
            _logger.Information("Server stopped.");
            if (_defaultLogger.IsValueCreated) _defaultLogger.Value?.Dispose();
            IsStop = true;
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
                    Stop();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
