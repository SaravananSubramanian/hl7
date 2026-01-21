using System;

namespace Com.SaravananSubramanian.Nhapi.BinaryData.CommonUtils
{
    public class BadBase64EncodingException : Exception
    {
        public BadBase64EncodingException(string errorMessage):base(errorMessage)
        {

        }
    }
}