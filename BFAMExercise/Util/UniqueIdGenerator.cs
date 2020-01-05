using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Util
{
    class UniqueIdGenerator
    {
        private long id = 0;

        private readonly object _lock = new object();

        public long GetId()
        {
            lock (_lock)
            {
                id++;
                return id;
            }
        }
    }
}
