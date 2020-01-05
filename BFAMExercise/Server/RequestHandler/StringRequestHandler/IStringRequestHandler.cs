using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Server.RequestHandler.StringRequestHandler
{
    interface IStringRequestHandler
    {
        string process(string req);
    }
}
