using System;

namespace BFAMExercise.RequestHandler
{
    interface IRequestHanlder
    {
        void ProcessRequest(string clientMsg, Action<string> send);
    }
}