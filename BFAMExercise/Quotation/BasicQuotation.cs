using BFAMExercise.PriceSource;
using BFAMExercise.QuoteEngine;
using BFAMExercise.Server.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BFAMExercise.Quotation
{
    class BasicQuotation : IBasicQuotation
    {
        private readonly IReferencePriceSource _referencePriceSource;
        private readonly IQuoteCalculationEngine _quoteCalEngine;

        public BasicQuotation(IReferencePriceSource rps, IQuoteCalculationEngine qce)
        {
            this._referencePriceSource = rps;
            this._quoteCalEngine = qce;
        }

        public double GetQuote(BasicQuoteRequestMessage msg)
        {
            double quote;

            try
            {
                double referencePrice = this._referencePriceSource.Get(msg.securityId);
                quote = this._quoteCalEngine.CalculateQuotePrice(msg.securityId, referencePrice, msg.isBuy, msg.quantity);
            }
            catch (Exception ex)
            {
                throw new BasicQuotationException("Fail to get quote.", ex);
            }

            return quote;
        }

        public async Task<double> GetQuoteAsync(BasicQuoteRequestMessage msg)
        {
            return await Task.Factory.StartNew(() => this.GetQuote(msg), TaskCreationOptions.LongRunning);
        }
    }
}
