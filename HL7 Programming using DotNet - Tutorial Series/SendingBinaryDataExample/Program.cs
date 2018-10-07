using System;
using System.Diagnostics;
using System.Text;
using NHapi.Base.Parser;
using NHapiTools.Base.Net;

namespace SendingBinaryDataExample
{
    public class NHapiSendOrderResponseMessageExample
    {
        private static int PORT_NUMBER = 57550;// change this to whatever your port number is
        public static void Main(String[] args)
        {
            try
            {
                // create the HL7 message
                // this OruMessageFactory class is not from NHAPI but my own wrapper class
                // check my GitHub page or see my earlier article for reference
                var oruMessage = OruMessageFactory.CreateMessage();

                // create a new MLLP client over the specified port (note this class is from NHAPI Tools)
                //Note that using higher level encodings such as UTF-16 is not recommended due to conflict with
                //MLLP wrapping characters

                var connection = new SimpleMLLPClient("localhost", PORT_NUMBER, Encoding.UTF8);

                // send the previously created HL7 message over the connection established
                var parser = new PipeParser();
                LogToDebugConsole("Sending ORU R01 message:" + "\n" + parser.Encode(oruMessage));
                var response = connection.SendHL7Message(oruMessage);

                // display the message response received from the remote party
                var responseString = parser.Encode(response);
                LogToDebugConsole("Received response:\n" + responseString);

            }
            catch (Exception e)
            {
                LogToDebugConsole($"Error occured while creating and transmitting HL7 message {e.Message}");
            }
        }

        private static void LogToDebugConsole(string informationToLog)
        {
            Debug.WriteLine(informationToLog);
        }

    }
}