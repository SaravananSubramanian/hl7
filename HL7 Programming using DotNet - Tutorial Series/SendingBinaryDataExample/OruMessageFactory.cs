using NHapi.Base.Model;

namespace SendingBinaryDataExample
{
    public class OruMessageFactory
    {
        //you will pass in parameters here in the form of a DTO or domain object
        //for message construction in your implementation
        public static IMessage CreateMessage()
        {
            return new OurOruR01MessageBuilder().Build();
        }
    }
}