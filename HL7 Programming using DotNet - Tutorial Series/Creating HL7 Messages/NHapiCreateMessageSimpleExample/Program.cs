using NHapi.Base.Model;
using NHapi.Base.Parser;

namespace NHapiCreateMessageSimpleExample;

/// <summary>
/// Demonstrates how to create HL7 V2 messages using NHAPI and serialize them
/// to both pipe-delimited and XML formats.
/// </summary>
public class Program
{
    private const string OutputDirectory = @"C:\HL7TestOutputs";

    public static void Main(string[] args)
    {
        try
        {
            // Create an ADT A01 (Admit/Visit Notification) message using our factory
            // The factory pattern allows easy extension to support other ADT message types
            Console.WriteLine("Creating ADT A01 message...");
            var adtMessage = AdtMessageFactory.CreateMessage("A01");

            // NHAPI provides multiple parsers for different encoding formats:
            // - PipeParser: Traditional HL7 pipe-delimited format (most common)
            // - DefaultXMLParser: HL7 XML encoding format
            var pipeParser = new PipeParser();
            var xmlParser = new DefaultXMLParser();

            Console.WriteLine("Message constructed successfully.\n");

            // Display the pipe-delimited message to console for verification
            Console.WriteLine("=== Pipe-Delimited Format ===");
            Console.WriteLine(pipeParser.Encode(adtMessage));
            Console.WriteLine();

            // Serialize to both formats
            WriteMessageFile(pipeParser, adtMessage, "testPipeDelimitedOutputFile.txt");
            WriteMessageFile(xmlParser, adtMessage, "testXmlOutputFile.xml");

            Console.WriteLine("\nMessage creation and serialization completed successfully.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating HL7 message: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Writes an HL7 message to a file using the specified parser encoding.
    /// </summary>
    /// <param name="parser">The parser to use for encoding (determines output format)</param>
    /// <param name="hl7Message">The HL7 message to serialize</param>
    /// <param name="outputFileName">The name of the output file</param>
    private static void WriteMessageFile(ParserBase parser, IMessage hl7Message, string outputFileName)
    {
        // Ensure output directory exists
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }

        var filePath = Path.Combine(OutputDirectory, outputFileName);

        Console.WriteLine($"Writing message to: {filePath}");

        // Encode the message and write to file
        // Note: Using UTF-8 encoding which is standard for HL7 V2 messages
        var encodedMessage = parser.Encode(hl7Message);
        File.WriteAllText(filePath, encodedMessage);

        Console.WriteLine($"Successfully wrote {encodedMessage.Length} characters to {outputFileName}");
    }
}
