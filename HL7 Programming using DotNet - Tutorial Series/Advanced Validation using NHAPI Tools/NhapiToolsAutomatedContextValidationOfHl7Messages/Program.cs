using System.Diagnostics;
using NHapi.Base.Parser;
using NHapiTools.Base.Validation;

namespace NhapiToolsAutomatedContextValidationOfHl7Messages
{
    public class Program
    {
        static void Main(string[] args)
        {
            DemonstrateNhapiToolsAutomatedContext();
        }

        private static void DemonstrateNhapiToolsAutomatedContext()
        {
            try
            {
                //Using an automated context should automatically load all validation rules in the assembly
                //use app.config setting key "NHapiRulesNamespace" to specify which .NET assembly to load for rules
                //here, we are including the message rule class "AdtA01MustHaveEvn4DataMessageRule" in the same assembly

                const string adtMessageToValidateAgainst =
                    "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|" +
                    "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
                    "EVN|A01|20110613083617|||\r" +
                    "PID|1||135769||MOUSE^MICKEY^||19281118|M|||123 Main St.^^Lake Buena Vista^FL^32830|" +
                    "|(407)939-1289^^^theMainMouse@disney.com|||||1719|99999999||||||||||||||||||||\r" +
                    "PV1|1|O|||||^^^^^^^^|^^^^^^^^"; ;

                LogToDebugConsole("Demonstration of NHAPI Tools Automated Context...");

                var parser = new PipeParser();

                //this should load the 
                var context = new AutomatedContext(parser.ValidationContext);
                parser.ValidationContext = context;

                //parse the message containing no event reason code data (EVN-4)
                //this should trigger a validation exception
                parser.Parse(adtMessageToValidateAgainst);
            }
            catch (System.Exception ex)
            {
                LogToDebugConsole("Validation exception thrown...");
                if (ex.InnerException != null)
                    LogToDebugConsole("Custom Validation: Message failed during parsing:" +
                                      ex.InnerException.Message);
            }
        }

        private static void LogToDebugConsole(string informationToLog)
        {
            Debug.WriteLine(informationToLog);
        }

    }
}
