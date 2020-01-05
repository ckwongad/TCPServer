using System;
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

        public TCPServer(string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();
        }

        public void Listen()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
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

        public void Stop()
        {
            server.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
