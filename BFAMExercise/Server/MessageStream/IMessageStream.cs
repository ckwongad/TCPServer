using System.Threading.Tasks;

namespace BFAMExercise.Server.MessageStream
{
    interface IMessageStream : System.IDisposable
    {
        string Read();
        Task<string> ReadAsync();
        void Write(string msg);
        Task WriteAsync(string msg);
        void Close();
    }
}