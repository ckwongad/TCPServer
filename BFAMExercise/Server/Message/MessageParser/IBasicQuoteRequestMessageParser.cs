namespace BFAMExercise.Server.Message.MessageParser
{
    interface IBasicQuoteRequestMessageParser
    {
        BasicQuoteRequestMessage Parse(string msg);
    }


    [System.Serializable]
    public class ParseQuoteRequestMessageException : System.Exception
    {
        public ParseQuoteRequestMessageException() { }
        public ParseQuoteRequestMessageException(string message) : base(message) { }
        public ParseQuoteRequestMessageException(string message, System.Exception inner) : base(message, inner) { }
        protected ParseQuoteRequestMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}