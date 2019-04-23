using System;
using System.Diagnostics;
using System.IO;
using NHapi.Base.Parser;
using NHapi.Base.Util;

namespace HapiTerserBasicOperations
{
    class HapiTerserBasicOperations
    {
        static void Main(string[] args)
        {
            try
            {
                // see my GitHub page for this file
                var messageString = ReadHl7MessageFromFileAsString(
                        "C:\\HL7TestInputFiles\\FileWithObservationResultMessage.txt");

                // instantiate a PipeParser, which handles the normal HL7 encoding
                var ourPipeParser = new PipeParser();

                // parse the message string into a Java message object
                var orderResultsHl7Message = ourPipeParser.Parse(messageString);

                // create a terser object instance by wrapping it around the message object
                var terser = new Terser(orderResultsHl7Message);

                // now, let us do various operations on the message
                var terserHelper = new OurTerserHelper(terser);

                var terserExpression = "MSH-6";
                var dataRetrieved = terserHelper.GetData(terserExpression);
                LogToDebugConsole($"Field 6 of MSH segment using expression '{terserExpression}' was '{dataRetrieved}'");

                terserExpression = "/.PID-5-2"; // notice the /. to indicate relative position to root node
                dataRetrieved = terserHelper.GetData(terserExpression);
                LogToDebugConsole($"Field 5 and Component 2 of the PID segment using expression '{terserExpression}' was {dataRetrieved}'");

                terserExpression = "/.*ID-5-2";
                dataRetrieved = terserHelper.GetData(terserExpression);
                LogToDebugConsole($"Field 5 and Component 2 of the PID segment using wildcard-based expression '{terserExpression}' was '{dataRetrieved}'");

                terserExpression = "/.P?D-5-2";
                dataRetrieved = terserHelper.GetData(terserExpression);
                LogToDebugConsole($"Field 5 and Component 2 of the PID segment using another wildcard-based expression '{terserExpression}' was '{dataRetrieved}'");

                terserExpression = "/.PV1-9(1)-1"; // note: field repetitions are zero-indexed
                dataRetrieved = terserHelper.GetData(terserExpression);
                LogToDebugConsole($"2nd repetition of Field 9 and Component 1 for it in the PV1 segment using expression '{terserExpression}' was '{dataRetrieved}'");

            }
            catch (Exception e)
            {
                LogToDebugConsole($"Error occured while creating HL7 message {e.Message}");
            }
        }

        public static string ReadHl7MessageFromFileAsString(string fileName) 
        {
            return File.ReadAllText(fileName);
        }

        private static void LogToDebugConsole(string informationToLog)
        {
            Debug.WriteLine(informationToLog);
        }
    }
}
