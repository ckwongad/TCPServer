using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.PriceSource
{
    /**
     * Source for reference prices.
     */
    public interface IReferencePriceSource
    {
        /**
         * Subscribe to changes to refernce prices.
         *
         * @param listener callback interface for changes
         */
        void Subscribe(IReferencePriceSourceListener listener);

        double Get(int securityId);
    }
}
