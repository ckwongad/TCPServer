using BFAMExercise.Server.Message;
using System.Threading.Tasks;

namespace BFAMExercise.Quotation
{
    interface IBasicQuotation
    {
        double GetQuote(BasicQuoteRequestMessage msg);
        Task<double> GetQuoteAsync(BasicQuoteRequestMessage msg);
    }


    [System.Serializable]
    public class BasicQuotationException : System.Exception
    {
        public BasicQuotationException() { }
        public BasicQuotationException(string message) : base(message) { }
        public BasicQuotationException(string message, System.Exception inner) : base(message, inner) { }
        protected BasicQuotationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}