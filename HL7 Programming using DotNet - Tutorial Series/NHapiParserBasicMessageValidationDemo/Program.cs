using System;
using System.Diagnostics;
using System.Linq;
using NHapi.Base.Parser;
using NHapi.Base.validation.impl;
using NHapi.Model.V24.Message;

namespace NHapiParserBasicMessageValidationDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //We will look at three scenarios here

            DemonstrateParsingValidMessage();

            DemonstrateDefaultValidationInNhapi();

            DemonstrateStrictValidationInNhapi();

            DemonstrateCustomValidationUsingNhapi();

        }

        private static void DemonstrateParsingValidMessage()
        {
            LogToDebugConsole("*Demonstration of validation of a correctly encoded HL7 message*");

            const string aValidAcknowledgementMessage =
                "MSH|^~\\&|SOME SENDING_APPLICATION|SOME SENDING_FACILITY|"
                + "SOME RECEIVING_APPLICATION|SOME RECEIVING_FACILITY|"
                + "20110614075841||ACK|1407511|P|2.4||||||\r\n"
                + "MSA|AA|1407511|Success||";

            var parser = new PipeParser {ValidationContext = new DefaultValidation()};
            var message = (ACK) parser.Parse(aValidAcknowledgementMessage);

            LogToDebugConsole($"Message version: {message.Version} ");

            LogToDebugConsole($"Sending application and facility info extracted: " +
                              $"{message.MSH.SendingApplication.Description} " +
                              $"{message.MSH.SendingFacility.Description} ");

            LogToDebugConsole($"Acknowledgement code and text extracted: " +
                              $"{message.MSA.AcknowledgementCode.Description} " +
                              $"{message.MSA.TextMessage.Description} ");
            LogToDebugConsole("Successfully validated a correct HL7 acknowledgement message");
        }

        private static void DemonstrateDefaultValidationInNhapi()
        {
            LogToDebugConsole("*Demonstration of default validation of a bad HL7 message*");

            //We are going to set the value in EVN-4 (Event Reason Code - 'IS' data type field)
            //to being longer than 200 characters. This should trigger an exception as part of
            //the default validation rules enabled in the NHAPI parser
            var eventReasonCode = "";
            for (var i = 0; i < 40; i++)
            {
                eventReasonCode += "RANDOMTEXT";
            }

            var anAdtMessageWithInvalidEvn4Field =
                "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|" +
                "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
                "EVN|A01|20110613083617||" + eventReasonCode + "|\r" +
                "PID|1||135769||MOUSE^MICKEY^||19281118|M|||123 Main St.^^Lake Buena Vista^FL^32830|" +
                "|(407)939-1289^^^theMainMouse@disney.com|||||1719|99999999||||||||||||||||||||\r" +
                "PV1|1|O|||||^^^^^^^^|^^^^^^^^";

            //make the parser use 'DefaultValidation' 
            //You don't have to specify it normally as it is the default
            var parser = new PipeParser {ValidationContext = new DefaultValidation()};
            try
            {
                parser.Parse(anAdtMessageWithInvalidEvn4Field);
            }
            catch (Exception e)
            {
                //An exception should be shown here as the EVN-4 ('IS' data type field)
                //has a length greater than 200 characters 
                LogToDebugConsole("Message failed during parsing:" + e.Message);
            }
        }

        private static void DemonstrateStrictValidationInNhapi()
        {
            LogToDebugConsole(
                "*Demonstration of strict validation of a bad HL7 message*");

            //We are going to set the value in PID-1 (Set ID – Patient ID - 'SI' data type field) to be a negative number
            //This should trigger an exception as part of the strict validation rules enabled in the NHAPI parser

            var anAdtMessageWithInvalidPid1Field =
                "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|" +
                "RECEIVING_APPLICATION|RECEIVING_FACILITY|" +
                "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
                "EVN|A01|20110613083617|||\r" +
                "PID|-1||135769||MOUSE^MICKEY^||19281118|M|||" +
                "123 Main St.^^Lake Buena Vista^FL^32830||(407)939-1289^^^theMainMouse@disney.com" +
                "|||||1719|99999999||||||||||||||||||||\r" +
                "PV1|1|O|||||^^^^^^^^|^^^^^^^^";

            //make the parser use 'StrictValidation'
            var parser = new PipeParser {ValidationContext = new StrictValidation()};
            try
            {
                parser.Parse(anAdtMessageWithInvalidPid1Field);
            }
            catch (Exception e)
            {
                //An exception should be shown here as PID-1 cannot be a negative number 
                LogToDebugConsole("Message failed during parsing:" + e.Message);
            }
        }

        private static void DemonstrateCustomValidationUsingNhapi()
        {
            LogToDebugConsole("*Demonstration of custom validation of a HL7 message*");

            //We are going to specify a message rule that EVN-4 (Event Reason Code) field is mandatory 
            //We do not have any data for the EVN-4 field here in order to trigger the validation exception

            var anAdtMessageWithMissingEvn4Field =
                "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|" +
                "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
                "EVN|A01|20110613083617|||\r" +
                "PID|1||135769||MOUSE^MICKEY^||19281118|M|||123 Main St.^^Lake Buena Vista^FL^32830|" +
                "|(407)939-1289^^^theMainMouse@disney.com|||||1719|99999999||||||||||||||||||||\r" +
                "PV1|1|O|||||^^^^^^^^|^^^^^^^^";
            ;

            //make the parser use 'StrictValidation'
            var parser = new PipeParser {ValidationContext = new OurCustomMessageValidation()};
            try
            {
                parser.Parse(anAdtMessageWithMissingEvn4Field);
            }
            catch (Exception e)
            {
                //An exception should be shown here as event reason code (EVN-4) was not supplied 
                if (e.InnerException != null)
                    LogToDebugConsole("Custom Validation: Message failed during parsing:" +
                                      e.InnerException.Message);
            }
        }

        private static void LogToDebugConsole(string informationToLog)
        {
            Debug.WriteLine(informationToLog);
        }

    }
}
