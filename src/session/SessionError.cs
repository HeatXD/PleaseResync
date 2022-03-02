using System;

namespace PleaseResync
{
    public class SessionError : Exception
    {
        public SessionError()
        {
        }

        public SessionError(string message) : base(message)
        {
        }
    }

    public class PredictionLimitError : SessionError
    {
        public PredictionLimitError(string message) : base(message)
        {
        }
    }
}
