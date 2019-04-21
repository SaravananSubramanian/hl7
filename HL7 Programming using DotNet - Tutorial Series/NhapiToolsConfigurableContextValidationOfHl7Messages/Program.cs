using System.Diagnostics;
using NHapi.Base.Parser;
using NHapiTools.Base.Validation;

namespace NhapiToolsConfigurableContextValidationOfHl7Messages
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Using a configurable context allows to control the rules used more precisely 

            //These need to be specified in the app.config file of the application
            //1.Use setting key "NHapiRulesNamespace" to specify which .NET assembly to look for rules
            //2.The "hl7ValidationRulesGroup\hl7ValidationRules" section specifies type of rules to look for
            //3.The "SpecificRulesGroup\SpecificRules" section specifies exact rule to apply on messages

            DemonstrateConfigurableContextEvn4FieldIsEmpty();
            DemonstrateConfigurableContextPidSegmentIsMissing();
        }

        private static void DemonstrateConfigurableContextEvn4FieldIsEmpty()
        {
            try
            {
                //ADT A01 message with ENV-4 field data left empty
                const string adtMessageToValidateAgainst =
                    "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|" +
                    "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
                    "EVN|A01|20110613083617|||\r" +
                    "PID|1||135769||MOUSE^MICKEY^||19281118|M|||123 Main St.^^Lake Buena Vista^FL^32830|" +
                    "|(407)939-1289^^^theMainMouse@disney.com|||||1719|99999999||||||||||||||||||||\r" +
                    "PV1|1|O|||||^^^^^^^^|^^^^^^^^"; ;

                LogToDebugConsole("Demonstration of NHAPI Tools Configurable Context...");
                LogToDebugConsole("Validation rule specified in app.config called 'MandatoryFieldADT_EVN-4' should fail here...");

                var parser = new PipeParser();

                //this should load the 
                var context = new ConfigurableContext(parser.ValidationContext);
                parser.ValidationContext = context;

                //parse the message containing no event reason code data (EVN-4)
                //this should trigger a validation exception
                parser.Parse(adtMessageToValidateAgainst);
            }
            catch (System.Exception ex)
            {
                LogToDebugConsole("Validation exception thrown...");
                if (ex.InnerException != null)
                    LogToDebugConsole("Configurable Context-based Validation: Message failed during parsing:" +
                                      ex.InnerException.Message);
            }
        }

        private static void DemonstrateConfigurableContextPidSegmentIsMissing()
        {
            try
            {
                //ADT A01 message with missing PID segment
                const string adtMessageWithPidSegmentMissing =
                    "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|" +
                    "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
                    "EVN|A01|20110613083617||Some Reason|\r" +
                    "PV1|1|O|||||^^^^^^^^|^^^^^^^^"; ;

                LogToDebugConsole("Demonstration of NHAPI Tools Configurable Context...");
                LogToDebugConsole("Validation rule specified in app.config called 'MandatorySegmentADT_A01_PID' should fail here...");

                var parser = new PipeParser();

                //this should load the 
                var context = new ConfigurableContext(parser.ValidationContext);
                parser.ValidationContext = context;

                //parse the message containing PID message segment
                //this should trigger a validation exception
                parser.Parse(adtMessageWithPidSegmentMissing);
            }
            catch (System.Exception ex)
            {
                LogToDebugConsole("Validation exception thrown...");
                if (ex.InnerException != null)
                    LogToDebugConsole("Configurable Context-based Validation: Message failed during parsing:" +
                                      ex.InnerException.Message);
            }
        }

        private static void LogToDebugConsole(string informationToLog)
        {
            Debug.WriteLine(informationToLog);
        }

    }
}
