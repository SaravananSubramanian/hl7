using System;

namespace CommonUtils
{
    public class BadBase64EncodingException : Exception
    {
        public BadBase64EncodingException(string errorMessage):base(errorMessage)
        {

        }
    }
}