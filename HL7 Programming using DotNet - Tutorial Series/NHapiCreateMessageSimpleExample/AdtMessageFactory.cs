using System;
using NHapi.Base.Model;

namespace NHapiCreateMessageSimpleExample
{
    public class AdtMessageFactory
    {
        public static IMessage CreateMessage(string messageType)
        {
            //This patterns enables you to build other message types 
            if (messageType.Equals("A01"))
            {
                return new OurAdtA01MessageBuilder().Build();
            }

            //if other types of ADT messages are needed, then implement your builders here
            throw new ArgumentException($"'{messageType}' is not supported yet. Extend this if you need to");
        }
    }
}