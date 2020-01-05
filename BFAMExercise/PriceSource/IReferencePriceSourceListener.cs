using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.PriceSource
{
    /**
     * Callback interface for {@link ReferencePriceSource}
     */
    public interface IReferencePriceSourceListener
    {

        /**
         * Called when a price has changed.
         *
         * @param securityId security identifier
         * @param price      reference price
         */
        void ReferencePriceChanged(int securityId, double price);
    }
}
