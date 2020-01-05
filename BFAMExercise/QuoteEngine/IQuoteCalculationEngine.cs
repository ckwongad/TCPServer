using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.QuoteEngine
{
    /**
     * Provides access to to libraries for pricing of quote requests.
     */
    public interface IQuoteCalculationEngine
    {

        /**
         * Calculate the appropriate price for a quote request on a security.
         * <p>
         * This may take a long time to execute.
         *
         * @param securityId     security identifier
         * @param referencePrice reference price to calculate theory price from (e.g. underlying security's price)
         * @param buy            {@code true} if buy, otherwise sell
         * @param quantity       quantity for quote
         * @return calculated quote price
         */
        double CalculateQuotePrice(int securityId, double referencePrice, bool buy, int quantity);
    }
}
