using System;
using System.Diagnostics;
using System.IO;
using HapiTerserBasicOperations;
using NHapi.Base.Parser;
using NHapi.Base.Util;

namespace HapiTerserAdvancedOperations
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //see my GitHub page for this file
                var messageString = ReadHl7MessageFromFileAsString("C:\\HL7TestInputFiles\\FileWithObservationResultMessage.txt");

                // instantiate a PipeParser, which handles the "traditional or default encoding"
                var ourPipeParser = new PipeParser();

                // parse the message string into a Java message object
                var orderResultsHl7Message = ourPipeParser.Parse(messageString);

                //create a terser object instance by wrapping it around the message object
                var terser = new Terser(orderResultsHl7Message);

                //now, let us do various operations on the message
                var terserDemonstrator = new OurTerserHelper(terser);

                //use a HL7 test utility such as HAPI Test Panel Utility as reference 
                //for a visual breakdown of these structures if you need to understand these terser expressions
                var terserExpression = "/RESPONSE/PATIENT/PID-5-1";
                var dataRetrieved = terserDemonstrator.GetData(terserExpression); 
                LogToDebugConsole($"Terser expression  '{terserExpression}' yielded '{dataRetrieved}'");

                terserExpression = "/RESPONSE/PATIENT/VISIT/PV1-9-3";
                dataRetrieved = terserDemonstrator.GetData(terserExpression);
                LogToDebugConsole($"Terser expression '{terserExpression}' yielded '{dataRetrieved}'");

                terserExpression = "/RESPONSE/ORDER_OBSERVATION(0)/OBSERVATION(1)/OBX-3";
                dataRetrieved = terserDemonstrator.GetData(terserExpression);
                LogToDebugConsole($"Terser expression '{terserExpression}' yielded '{dataRetrieved}'");

                terserExpression = "/.ORDER_OBSERVATION(0)/ORC-12-3";
                dataRetrieved = terserDemonstrator.GetData(terserExpression);
                LogToDebugConsole($"Terser expression '{terserExpression}' yielded '{dataRetrieved}'");

                //let us now try a set operation using the terser
                terserExpression = "/.OBSERVATION(0)/NTE-3";
                terserDemonstrator.SetData(terserExpression, "This is our override value using the setter");
                LogToDebugConsole("Set the data for second repetition of the NTE segment and its Third field..");

                LogToDebugConsole("\nWill display our modified message below \n");
                LogToDebugConsole(ourPipeParser.Encode(orderResultsHl7Message));

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
