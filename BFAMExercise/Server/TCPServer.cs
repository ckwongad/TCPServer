using Amib.Threading;
using AsyncAwaitBestPractices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BFAMExercise.Server
{
    public class TCPServer
    {
        private readonly TcpListener server = null;
        private readonly ConcurrentDictionary<long, TCPSession> sessions = new ConcurrentDictionary<long, TCPSession>();
        private readonly SmartThreadPool _smartThreadPool = new SmartThreadPool();

        #region Properties
        private volatile bool _isStop = false;
        public bool IsStop
        {
            get { return _isStop; }
            private set { _isStop = value; }
        }
        #endregion Properties

        #region Events
        public event EventHandler<TcpClient> OnNewConnection;
        #endregion Events

        #region Delegates
        private Action<string, Action<string>> _requestHandling;
        #endregion Delegates

        public TCPServer(string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();

            _smartThreadPool.MaxThreads = 25;
        }

        public async void ListenAsync()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = await server.AcceptTcpClientAsync();
                    OnNewConnection?.Invoke(this, client);
                    Console.WriteLine("Connected!");

                    CreateSession(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                Stop();
            }
        }

        public void Listen()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
                    OnNewConnection?.Invoke(this, client);
                    Console.WriteLine("Connected!");

                    CreateSession(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                Stop();
            }
        }

        private void CreateSession(TcpClient client)
        {
            try
            {
                var session = new TCPSession(client);
                if (sessions.TryAdd(session.SessionId, session))
                {
                    session.OnClose += (sender, sessionId) => {
                        if (!sessions.TryRemove(sessionId, out var tmp))
                            Console.WriteLine("Couldn't remove session with session id: {0}.", sessionId);
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
                                Console.WriteLine("Error in handling request. {0}", e);
                                session.Close();
                            }
                        });
                    };
                    session.StartAsync();
                    //Thread t = new Thread(session.Start);
                    //t.Start();
                    Console.WriteLine("Session #{0} started. Total # of sessions: {1}", session.SessionId, sessions.Count);
                }
                else
                {
                    Console.WriteLine("Couldn't add session with session id: {0}.", session.SessionId);
                    session.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create session: {0}.", e);
            }
        }

        public void RegisterRequestHandler(Action<string, Action<string>> requestHandler)
        {
            this._requestHandling = requestHandler;
        }

        public void WriteReport()
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
            Console.WriteLine(reportTemplate, maxT - AvailableT, totalThreads, sessions.Count, _smartThreadPool.InUseThreads, _smartThreadPool.ActiveThreads);

        }

        public void Stop()
        {
            if (IsStop) return;

            server.Stop();
            Console.WriteLine("Server stopped.");
            IsStop = true;
        }
    }
}
