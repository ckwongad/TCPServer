using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.QuoteEngine
{
    class ProdAQuoteCalculationEngine : IQuoteCalculationEngine
    {
        double IQuoteCalculationEngine.CalculateQuotePrice(int securityId, double referencePrice, bool buy, int quantity)
        {
            FindPrimeNumber(10000);

            if (buy)
                return referencePrice * quantity;
            else
                return -referencePrice * quantity;
        }

        private long FindPrimeNumber(int n)
        {
            int count = 0;
            long a = 2;
            while (count < n)
            {
                long b = 2;
                int prime = 1;// to check if found a prime
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
            return (--a);
        }
    }
}
