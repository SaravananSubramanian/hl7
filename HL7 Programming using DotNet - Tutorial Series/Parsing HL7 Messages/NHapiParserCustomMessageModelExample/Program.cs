using System.Diagnostics;
using NHapi.Base.Parser;
using NHapi.Model.CustomZSegments;

namespace NHapiParserCustomMessageModelExample
{
	public class Program
	{
		public static void Main(string[] args)
		{
            const string customSegmentBasedHl7Message = "MSH|^~\\&|SUNS1|OVI02|AZIS|CMD|200606221348||ADT^A01|1049691900|P|2.3\r"
			                                            + "EVN|A01|200803051509||||200803031508\r"
			                                            + "PID|||5520255^^^PK^PK~ZZZZZZ83M64Z148R^^^CF^CF~ZZZZZZ83M64Z148R^^^SSN^SSN^^20070103^99991231~^^^^TEAM||ZZZ^ZZZ||19830824|F||||||||||||||||||||||N\r"
			                                            + "ZPV|Some Custom Notes|Additional custom description of the visit goes here";

	        var parser = new PipeParser();

		    LogToDebugConsole("Attempting to parse message string into HL7 message object...");

            var parsedMessage = parser.Parse(customSegmentBasedHl7Message, Constants.Version);

		    LogToDebugConsole("Parsed message into generic ADT_A01 message successfully...");

            LogToDebugConsole("Type: " + parsedMessage.GetType());

		    LogToDebugConsole("Casting the parsed message object into our custom ADT_A01 message...");

            //cast this to the custom message that we have overridden
            var zdtA01 = parsedMessage as NHapi.Model.CustomZSegments.Message.ADT_A01;  
	        if (zdtA01 != null)
	        {
	            LogToDebugConsole($"Custom Notes retrieved from ZPV segment was -> {zdtA01.ZPV.CustomNotes.Value}");
                LogToDebugConsole($"Custom Description retrieved from ZPV segment was -> {zdtA01.ZPV.CustomDescription.Value}");
	            LogToDebugConsole("Entire parse operation completed successfully...");
            }
	    }

	    private static void LogToDebugConsole(string informationToLog)
	    {
	        Debug.WriteLine(informationToLog);
	    }
    }
}
