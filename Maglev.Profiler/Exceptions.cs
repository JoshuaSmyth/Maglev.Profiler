using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaglevProfiler
{
    public class UnableToDecodeMessageException : Exception
    {
        public UnableToDecodeMessageException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }

    public class UnknownMessageException : Exception
    {
        public UnknownMessageException(string message) 
            : base(message)
        {
            
        }

        public UnknownMessageException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}
