using NHapi.Base.Parser;
using NHapi.Model.V23.Message;

namespace Com.SaravananSubramanian.Nhapi.Parsing.BasicOperations;

/// <summary>
/// Demonstrates basic HL7 message parsing operations using NHAPI.
/// Shows how to parse messages, access segment data, modify values,
/// and convert between pipe-delimited and XML formats.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // Sample ACK (Acknowledgment) message in pipe-delimited format
        // ACK messages are sent in response to other HL7 messages to confirm receipt
        const string sampleAckMessage =
            "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|20110614075841||ACK|1407511|P|2.3||||||\r" +
            "MSA|AA|1407511|Success||";

        Console.WriteLine("=== NHAPI Parser Basic Operations Demo ===\n");

        try
        {
            // Demonstrate parsing and message manipulation
            ParseAndDisplayMessage(sampleAckMessage);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing HL7 message: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Parses an HL7 message and demonstrates common operations.
    /// </summary>
    private static void ParseAndDisplayMessage(string messageString)
    {
        // PipeParser handles the traditional pipe-delimited HL7 encoding
        // This is the most common format used in healthcare integrations
        var pipeParser = new PipeParser();

        Console.WriteLine("1. Parsing pipe-delimited message...\n");

        // Parse the string into a message object
        // The parser automatically determines the message type from MSH-9
        var parsedMessage = pipeParser.Parse(messageString);

        // Display basic information about the parsed message
        Console.WriteLine($"   Message Type: {parsedMessage.GetStructureName()}");
        Console.WriteLine($"   Message Version: {parsedMessage.Version}");
        Console.WriteLine();

        // Cast to the specific message type to access typed segment accessors
        // Using pattern matching for safe casting
        if (parsedMessage is ACK ackMessage)
        {
            DisplayAckMessageData(ackMessage);
            ModifyAndRedisplayMessage(ackMessage, pipeParser);
            ConvertToXmlFormat(ackMessage);
        }
        else
        {
            Console.WriteLine($"   Unexpected message type: {parsedMessage.GetType().Name}");
        }
    }

    /// <summary>
    /// Displays data from an ACK message's segments.
    /// </summary>
    private static void DisplayAckMessageData(ACK ackMessage)
    {
        Console.WriteLine("2. Extracting data from message segments...\n");

        // Access MSH (Message Header) segment data
        var msh = ackMessage.MSH;
        Console.WriteLine("   MSH Segment Data:");
        Console.WriteLine($"   - Message Type: {msh.MessageType.MessageType.Value}");
        Console.WriteLine($"   - Message Control ID: {msh.MessageControlID.Value}");
        Console.WriteLine($"   - Sending Application: {msh.SendingApplication.NamespaceID.Value}");
        Console.WriteLine($"   - Sending Facility: {msh.SendingFacility.NamespaceID.Value}");
        Console.WriteLine($"   - Message Timestamp: {msh.DateTimeOfMessage.TimeOfAnEvent.Value}");
        Console.WriteLine();

        // Access MSA (Message Acknowledgment) segment data
        var msa = ackMessage.MSA;
        Console.WriteLine("   MSA Segment Data:");
        Console.WriteLine($"   - Acknowledgment Code: {msa.AcknowledgementCode.Value}");
        Console.WriteLine($"   - Message Control ID: {msa.MessageControlID.Value}");
        Console.WriteLine($"   - Text Message: {msa.TextMessage.Value}");
        Console.WriteLine();

        // Explain the acknowledgment codes
        Console.WriteLine("   Note: ACK codes meanings:");
        Console.WriteLine("   - AA = Application Accept (message processed successfully)");
        Console.WriteLine("   - AE = Application Error (error in message content)");
        Console.WriteLine("   - AR = Application Reject (message rejected)");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates modifying message data and re-encoding.
    /// </summary>
    private static void ModifyAndRedisplayMessage(ACK ackMessage, PipeParser pipeParser)
    {
        Console.WriteLine("3. Modifying message data...\n");

        // Change the acknowledgment code from AA (Accept) to AR (Reject)
        // In real scenarios, this would be based on processing results
        var originalCode = ackMessage.MSA.AcknowledgementCode.Value;
        ackMessage.MSA.AcknowledgementCode.Value = "AR";
        ackMessage.MSA.TextMessage.Value = "Message rejected due to validation errors";

        Console.WriteLine($"   Changed acknowledgment code from '{originalCode}' to 'AR'");
        Console.WriteLine();

        // Re-encode the modified message
        Console.WriteLine("   Modified message (pipe-delimited):");
        Console.WriteLine($"   {pipeParser.Encode(ackMessage)}");
        Console.WriteLine();
    }

    /// <summary>
    /// Converts the message to XML format for display.
    /// </summary>
    private static void ConvertToXmlFormat(ACK ackMessage)
    {
        Console.WriteLine("4. Converting to XML format...\n");

        // DefaultXMLParser handles HL7 XML encoding
        // XML format is sometimes preferred for web services or logging
        var xmlParser = new DefaultXMLParser();
        var xmlOutput = xmlParser.Encode(ackMessage);

        Console.WriteLine("   XML Formatted Message:");
        Console.WriteLine(xmlOutput);
    }
}
