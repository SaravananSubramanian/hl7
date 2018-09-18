using System;
using System.IO;
using NHapi.Base.Model;
using NHapi.Base.Parser;

namespace NHapiCreateMessageSimpleExample
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // create the HL7 message
                // this AdtMessageFactory class is not from NHAPI but my own wrapper
                Console.WriteLine("Creating ADT A01 message...");
                var adtMessage = AdtMessageFactory.CreateMessage("A01");

                // create these parsers for the file encoding operations
                var pipeParser = new PipeParser();
                var xmlParser = new DefaultXMLParser();

                // print out the message that we constructed
                Console.WriteLine("Message was constructed successfully..." + "\n");

                // serialize the message to pipe delimited output file
                WriteMessageFile(pipeParser, adtMessage, "C:\\HL7TestOutputs", "testPipeDelimitedOutputFile.txt");

                // serialize the message to XML format output file
                WriteMessageFile(xmlParser, adtMessage, "C:\\HL7TestOutputs", "testXmlOutputFile.xml");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while creating HL7 message {e.Message}");
            }
        }

        private static void WriteMessageFile(ParserBase parser, IMessage hl7Message, string outputDirectory, string outputFileName)
        {
            if (Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var fileName = Path.Combine(outputDirectory, outputFileName);

            if(File.Exists(fileName))
                File.Delete(fileName);
            File.WriteAllText(fileName, parser.Encode(hl7Message));
        }
    }
}
