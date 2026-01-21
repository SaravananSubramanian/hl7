using NHapi.Base.Model;

namespace Com.SaravananSubramanian.Nhapi.BinaryData.Sending
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