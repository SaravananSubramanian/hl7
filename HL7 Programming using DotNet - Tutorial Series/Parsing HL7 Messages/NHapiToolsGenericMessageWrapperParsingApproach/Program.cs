using System.Diagnostics;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapiTools.Base.Model;
using NHapiTools.Base.Parser;

namespace NHapiToolsGenericMessageWrapperParsingApproach
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string customSegmentBasedHl7Message = "MSH|^~\\&|SUNS1|OVI02|AZIS|CMD|200606221348||ADT^A01|1049691900|P|2.3\r"
                                                        + "EVN|A01|200803051509||||200803031508\r"
                                                        + "PID|||5520255^^^PK^PK~ZZZZZZ83M64Z148R^^^CF^CF~ZZZZZZ83M64Z148R^^^SSN^SSN^^20070103^99991231~^^^^TEAM||ZZZ^ZZZ||19830824|F||||||||||||||||||||||N\r"
                                                        + "ZPV|Some Custom Notes|Additional custom description of the visit goes here";

            var enhancedModelClassFactory = new EnhancedModelClassFactory();
            var pipeParser = new PipeParser(enhancedModelClassFactory);
            enhancedModelClassFactory.ValidationContext = pipeParser.ValidationContext;

            LogToDebugConsole("Attempting to parse message string into HL7 message object...");

            var ourHl7Message = pipeParser.Parse(customSegmentBasedHl7Message);

            LogToDebugConsole("Parsed message into generic ADT_A01 message successfully...");

            LogToDebugConsole("Unwrapping payload from our custom ADT_A01 message using GenericMessageWrapper class...");

            var wrappedGenericMessage = ourHl7Message as GenericMessageWrapper;
            var originalMessage = wrappedGenericMessage?.Unwrap();

            if (wrappedGenericMessage?.GetSegment<ISegment>("ZPV") != null)
            {
                LogToDebugConsole("Casting unwrapped payload into our custom ADT A01 class...");
                //casting to our custom class and retrieving the custom segment within it
                var zpvSegment = ((NHapi.Model.CustomZSegments.Message.ADT_A01)originalMessage).ZPV;
                LogToDebugConsole($"Custom Notes retrieved from ZPV segment was -> {zpvSegment.CustomNotes.Value}");
                LogToDebugConsole($"Custom Description retrieved from ZPV segment was -> {zpvSegment.CustomDescription.Value}");
                LogToDebugConsole("Entire parse operation completed successfully...");
            }
        }

        private static void LogToDebugConsole(string informationToLog)
        {
            Debug.WriteLine(informationToLog);
        }
    }
}
