using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.PriceSource
{
    /**
     * A random price source that returns a price between 0 and 100
     */
    public class RandomReferencePriceSource : IReferencePriceSource
    {
        private static Random _random = new Random();

        public double Get(int securityId)
        {
            return _random.NextDouble() * 100;
        }

        public void Subscribe(IReferencePriceSourceListener listener)
        {
            throw new NotImplementedException();
        }
    }
}
