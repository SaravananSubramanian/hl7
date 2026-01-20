using System.Text;
using NHapi.Base.Parser;
using NHapiTools.Base.Net;

namespace NHapiSendMessageSimpleExample;

/// <summary>
/// Demonstrates sending HL7 V2 messages over MLLP (Minimal Lower Layer Protocol).
/// Uses NHapiTools' SimpleMLLPClient for network communication.
/// </summary>
/// <remarks>
/// MLLP is the standard transport protocol for HL7 V2 messages over TCP/IP.
/// It wraps messages with special characters:
/// - Start Block (SB): 0x0B
/// - End Block (EB): 0x1C
/// - Carriage Return (CR): 0x0D
///
/// Message format: [SB]message[EB][CR]
///
/// Before running this example, ensure an HL7 listener is running
/// on the specified host and port (e.g., using the MLLP server example).
/// </remarks>
public class Program
{
    // Configuration - adjust these values for your environment
    private const string RemoteHost = "localhost";
    private const int RemotePort = 52463;

    public static void Main(string[] args)
    {
        Console.WriteLine("=== NHAPI Send Message Example ===\n");
        Console.WriteLine($"Target: {RemoteHost}:{RemotePort}\n");

        try
        {
            SendHl7Message();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("\nMake sure an HL7 listener is running on the target host and port.");
        }
    }

    /// <summary>
    /// Creates and sends an ADT A01 message, then processes the response.
    /// </summary>
    private static void SendHl7Message()
    {
        // Create an ADT A01 message using our factory
        Console.WriteLine("1. Creating ADT A01 message...");
        var adtMessage = AdtMessageFactory.CreateMessage("A01");

        // Get the pipe-delimited representation for display
        var pipeParser = new PipeParser();
        var outgoingMessage = pipeParser.Encode(adtMessage);

        Console.WriteLine("\n2. Outgoing message:");
        Console.WriteLine(FormatMessageForDisplay(outgoingMessage));

        // Create MLLP client connection
        // Note: UTF-8 is recommended. Higher-level encodings (UTF-16, UTF-32)
        // can conflict with MLLP's special wrapper characters.
        Console.WriteLine($"\n3. Connecting to {RemoteHost}:{RemotePort}...");

        using var connection = new SimpleMLLPClient(RemoteHost, RemotePort, Encoding.UTF8);

        // Send the message and wait for ACK response
        Console.WriteLine("4. Sending message and waiting for response...");
        var response = connection.SendHL7Message(adtMessage);

        // Display the response
        Console.WriteLine("\n5. Response received:");
        var responseString = pipeParser.Encode(response);
        Console.WriteLine(FormatMessageForDisplay(responseString));

        // Parse response to check acknowledgment status
        AnalyzeAckResponse(response);
    }

    /// <summary>
    /// Analyzes the ACK response to determine if the message was accepted.
    /// </summary>
    private static void AnalyzeAckResponse(NHapi.Base.Model.IMessage response)
    {
        Console.WriteLine("\n6. Analyzing response...");

        // Try to extract acknowledgment code from MSA segment
        try
        {
            var terser = new NHapi.Base.Util.Terser(response);
            var ackCode = terser.Get("MSA-1");
            var messageControlId = terser.Get("MSA-2");
            var textMessage = terser.Get("MSA-3");

            Console.WriteLine($"   Acknowledgment Code: {ackCode}");
            Console.WriteLine($"   Message Control ID: {messageControlId}");

            if (!string.IsNullOrEmpty(textMessage))
            {
                Console.WriteLine($"   Text Message: {textMessage}");
            }

            // Interpret the acknowledgment code
            var status = ackCode switch
            {
                "AA" => "Application Accept - Message processed successfully",
                "AE" => "Application Error - Message has errors",
                "AR" => "Application Reject - Message rejected",
                "CA" => "Commit Accept - Message committed",
                "CE" => "Commit Error - Message has errors",
                "CR" => "Commit Reject - Message rejected",
                _ => $"Unknown acknowledgment code: {ackCode}"
            };

            Console.WriteLine($"   Status: {status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Unable to analyze response: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats an HL7 message for console display by replacing
    /// carriage returns with newlines and adding indentation.
    /// </summary>
    private static string FormatMessageForDisplay(string message)
    {
        var lines = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(Environment.NewLine, lines.Select(line => $"   {line}"));
    }
}
