using BFAMExercise.Server.MessageStream;
using Serilog;
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
        private static readonly Random rn = new Random();

        private int _beeId, _numRequests;
        private DelimiterMessageStream _msgStream;
        private Stopwatch _sw = Stopwatch.StartNew();
        private Dictionary<int, long> _startTimes = new Dictionary<int, long>();
        private ILogger _logger;

        public int MaxDelayTime { get; set; } = 500;

        public BeeWithGuns(int id, int numRequests)
            : this(id, numRequests, Log.Logger)
        {
        }

        public BeeWithGuns(int id, int numRequests, ILogger logger)
        {
            _beeId = id;
            _numRequests = numRequests;
            _logger = logger;
        }

        public async Task<bool> AttackAsync()
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
                            await Task.Delay(rn.Next(0, MaxDelayTime));
                            var message = _beeId + partialMessage + requestId;
                            _startTimes.Add(requestId, _sw.ElapsedMilliseconds);
                            await _msgStream.WriteAsync(message);
                            _logger.Information("Cient {0} Sent: {1}", _beeId, message);
                        }

                        var isSuccess = await CheckReponseAsync();

                        _msgStream.Close();
                        tcpClient.Close();

                        return isSuccess;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Client {0}: {1}.", _beeId, e);
                return false;
            }
            finally
            {
                _sw.Stop();
            }
        }

        private async Task<bool> CheckReponseAsync()
        {
            string response = "";
            try
            {
                int count = 0;
                while (count++ < _numRequests)
                {
                    response = await _msgStream.ReadAsync().ConfigureAwait(false);
                    var startTime = _startTimes.GetValueOrDefault(int.Parse(response.Split(' ')[0]));
                    _logger.Information("Cient {0} Received: {1}. Process time(ms): {2}",
                        _beeId, response, _sw.ElapsedMilliseconds - startTime);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Cient {0} encounters error in checking reponse: {1}. {2}", _beeId, response, e);
                return false;
            }
        }
    }
}
