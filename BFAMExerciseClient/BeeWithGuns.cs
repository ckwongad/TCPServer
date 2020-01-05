using BFAMExercise.Server.MessageStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BFAMExerciseClient
{
    class BeeWithGuns
    {
        private int _beeId, _numRequests;
        private DelimiterMessageStream _msgStream;
        private Stopwatch _sw = Stopwatch.StartNew();
        private Dictionary<int, long> _startTimes = new Dictionary<int, long>();

        public BeeWithGuns(int id, int numRequests)
        {
            _beeId = id;
            _numRequests = numRequests;
        }

        public async Task Attack()
        {
            try
            {
                using (var tcpClient = new TcpClient("127.0.0.1", 3000))
                {
                    using (_msgStream = new DelimiterMessageStream(tcpClient.GetStream()))
                    {
                        string partialMessage = " BUY ";

                        _sw.Start();

                        int requestId = 0;
                        while (requestId++ < _numRequests)
                        {
                            var message = _beeId + partialMessage + requestId;
                            _startTimes.Add(requestId, _sw.ElapsedMilliseconds);
                            await _msgStream.WriteAsync(message);
                            Console.WriteLine("Cient {0} Sent: {1}", _beeId, message);
                        }

                        await CheckReponse();

                        _msgStream.Close();
                        tcpClient.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client {0}: {1}.", _beeId, e);
            }
            finally
            {
                _sw.Stop();
            }
        }

        private async Task CheckReponse()
        {
            string response = "";
            try
            {
                int count = 0;
                while (count++ < _numRequests)
                {
                    response = await _msgStream.ReadAsync().ConfigureAwait(false);
                    var startTime = _startTimes.GetValueOrDefault(int.Parse(response.Split(' ')[0]));
                    Console.WriteLine("Cient {0} Received: {1}. Process time(ms): {2}",
                        _beeId, response, _sw.ElapsedMilliseconds - startTime);
                    int maxT, AvailableT, tmp;
                    ThreadPool.GetMaxThreads(out maxT, out tmp);
                    ThreadPool.GetAvailableThreads(out AvailableT, out tmp);
                    int totalThreads = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
                    Console.WriteLine("Active threads in threadpool: {0}. Total thread: {1}.", maxT - AvailableT, totalThreads);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cient {0} encounters error in checking reponse: {1}. {2}", _beeId, response, e);
            }
        }
    }
}
