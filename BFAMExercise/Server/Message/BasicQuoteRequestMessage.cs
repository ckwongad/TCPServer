using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Server.Message
{
    public struct BasicQuoteRequestMessage
    {
        public int securityId;
        public bool isBuy;
        public int quantity;

        public BasicQuoteRequestMessage(int securityId, bool buy, int quantity)
        {
            this.securityId = securityId;
            this.isBuy = buy;
            this.quantity = quantity;
        }
    }
}
