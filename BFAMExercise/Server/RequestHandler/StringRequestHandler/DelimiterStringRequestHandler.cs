using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Server.RequestHandler.StringRequestHandler
{
    class DelimiterStringRequestHandler : IStringRequestHandler
    {
        public string Delimiter { get; }

        DelimiterStringRequestHandler(string delimiter)
        {
            this.Delimiter = delimiter;
        }

        public string process(string req)
        {
            throw new NotImplementedException();
        }
    }
}
