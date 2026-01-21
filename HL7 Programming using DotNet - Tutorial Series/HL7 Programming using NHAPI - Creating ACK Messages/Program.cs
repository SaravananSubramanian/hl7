using System.Globalization;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Model.V23.Message;
using NHapi.Model.V23.Segment;

namespace Com.SaravananSubramanian.Nhapi.AckMessages;

/// <summary>
/// Demonstrates how to create ACK (Acknowledgment) messages in response to incoming HL7 messages.
/// ACK messages are fundamental to HL7 communication as they confirm receipt and processing status.
/// </summary>
/// <remarks>
/// HL7 Acknowledgment Codes:
/// - AA: Application Accept - Message processed successfully
/// - AE: Application Error - Error in message content (syntax/semantic)
/// - AR: Application Reject - Message rejected (application cannot process)
/// - CA: Commit Accept - Message committed to safe storage
/// - CE: Commit Error - Commit error
/// - CR: Commit Reject - Commit rejected
///
/// The ACK message contains:
/// - MSH: Message Header (mirrors original with response details)
/// - MSA: Message Acknowledgment (acknowledgment code and reference to original message)
/// - ERR: Error segment (optional, for error details)
/// </remarks>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== NHAPI ACK Message Generation Example ===\n");

        // Sample incoming ADT A01 message to acknowledge
        const string incomingMessage =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20240115120000||ADT^A01|MSG001|P|2.3|||AL|NE|\r" +
            "EVN|A01|20240115120000|||\r" +
            "PID|1||12345^^^HOSP^MR||DOE^JOHN^A||19800101|M|||||||||||\r" +
            "PV1|1|I|ICU^101^A|||||||||||||||||||||||||||||||||||||||";

        try
        {
            var pipeParser = new PipeParser();

            // Parse the incoming message
            Console.WriteLine("1. Incoming message to acknowledge:");
            Console.WriteLine(FormatMessageForDisplay(incomingMessage));
            Console.WriteLine();

            var parsedMessage = pipeParser.Parse(incomingMessage);

            // Demonstrate different ACK scenarios
            DemonstrateSuccessfulAck(parsedMessage, pipeParser);
            DemonstrateErrorAck(parsedMessage, pipeParser);
            DemonstrateRejectAck(parsedMessage, pipeParser);

            Console.WriteLine("\n=== ACK Generation Complete ===");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Creates an AA (Application Accept) acknowledgment for successful processing.
    /// </summary>
    private static void DemonstrateSuccessfulAck(IMessage originalMessage, PipeParser parser)
    {
        Console.WriteLine("2. Creating AA (Application Accept) ACK:");
        Console.WriteLine("   Use when message was processed successfully.\n");

        var ack = AckMessageBuilder.CreateAck(
            originalMessage,
            AcknowledgmentCode.AA,
            "Message processed successfully");

        Console.WriteLine("   Generated ACK:");
        Console.WriteLine(FormatMessageForDisplay(parser.Encode(ack)));
        Console.WriteLine();
    }

    /// <summary>
    /// Creates an AE (Application Error) acknowledgment for processing errors.
    /// </summary>
    private static void DemonstrateErrorAck(IMessage originalMessage, PipeParser parser)
    {
        Console.WriteLine("3. Creating AE (Application Error) ACK:");
        Console.WriteLine("   Use when there's an error in message content.\n");

        var ack = AckMessageBuilder.CreateAck(
            originalMessage,
            AcknowledgmentCode.AE,
            "Patient ID 12345 not found in system");

        Console.WriteLine("   Generated ACK:");
        Console.WriteLine(FormatMessageForDisplay(parser.Encode(ack)));
        Console.WriteLine();
    }

    /// <summary>
    /// Creates an AR (Application Reject) acknowledgment for rejected messages.
    /// </summary>
    private static void DemonstrateRejectAck(IMessage originalMessage, PipeParser parser)
    {
        Console.WriteLine("4. Creating AR (Application Reject) ACK:");
        Console.WriteLine("   Use when message is rejected (e.g., unsupported message type).\n");

        var ack = AckMessageBuilder.CreateAck(
            originalMessage,
            AcknowledgmentCode.AR,
            "ADT A01 messages are not accepted by this system",
            "MSH",
            "9",
            "Message type not supported");

        Console.WriteLine("   Generated ACK:");
        Console.WriteLine(FormatMessageForDisplay(parser.Encode(ack)));
    }

    private static string FormatMessageForDisplay(string message)
    {
        var lines = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(Environment.NewLine, lines.Select(line => $"   {line}"));
    }
}

/// <summary>
/// Enumeration of HL7 acknowledgment codes.
/// </summary>
public enum AcknowledgmentCode
{
    /// <summary>Application Accept - Message processed successfully</summary>
    AA,
    /// <summary>Application Error - Error in message content</summary>
    AE,
    /// <summary>Application Reject - Message rejected</summary>
    AR,
    /// <summary>Commit Accept - Message committed to safe storage</summary>
    CA,
    /// <summary>Commit Error - Commit error</summary>
    CE,
    /// <summary>Commit Reject - Commit rejected</summary>
    CR
}

/// <summary>
/// Builder class for creating ACK (Acknowledgment) messages.
/// </summary>
public static class AckMessageBuilder
{
    /// <summary>
    /// Creates an ACK message in response to the original message.
    /// </summary>
    /// <param name="originalMessage">The original message being acknowledged</param>
    /// <param name="ackCode">The acknowledgment code (AA, AE, AR, etc.)</param>
    /// <param name="textMessage">Optional text message describing the result</param>
    /// <param name="errorSegmentId">Optional: Segment where error occurred</param>
    /// <param name="errorFieldPosition">Optional: Field position of error</param>
    /// <param name="errorMessage">Optional: Detailed error message</param>
    /// <returns>A fully constructed ACK message</returns>
    public static ACK CreateAck(
        IMessage originalMessage,
        AcknowledgmentCode ackCode,
        string? textMessage = null,
        string? errorSegmentId = null,
        string? errorFieldPosition = null,
        string? errorMessage = null)
    {
        var ack = new ACK();

        // Build MSH segment (response header)
        BuildMshSegment(ack.MSH, originalMessage);

        // Build MSA segment (acknowledgment details)
        BuildMsaSegment(ack.MSA, originalMessage, ackCode, textMessage);

        // Build ERR segment if error details are provided
        if (!string.IsNullOrEmpty(errorSegmentId) || !string.IsNullOrEmpty(errorMessage))
        {
            BuildErrSegment(ack.ERR, errorSegmentId, errorFieldPosition, errorMessage);
        }

        return ack;
    }

    private static void BuildMshSegment(MSH msh, IMessage originalMessage)
    {
        // Extract original MSH for reference
        var originalMsh = (MSH)originalMessage.GetStructure("MSH");

        // Standard encoding
        msh.FieldSeparator.Value = "|";
        msh.EncodingCharacters.Value = "^~\\&";

        // Swap sending and receiving (response goes back to sender)
        msh.SendingApplication.NamespaceID.Value = originalMsh.ReceivingApplication.NamespaceID.Value;
        msh.SendingFacility.NamespaceID.Value = originalMsh.ReceivingFacility.NamespaceID.Value;
        msh.ReceivingApplication.NamespaceID.Value = originalMsh.SendingApplication.NamespaceID.Value;
        msh.ReceivingFacility.NamespaceID.Value = originalMsh.SendingFacility.NamespaceID.Value;

        // Set response timestamp
        msh.DateTimeOfMessage.TimeOfAnEvent.Value =
            DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        // Message type is ACK
        msh.MessageType.MessageType.Value = "ACK";
        msh.MessageType.TriggerEvent.Value = originalMsh.MessageType.TriggerEvent.Value;

        // Generate new message control ID for the ACK
        msh.MessageControlID.Value = $"ACK{DateTime.Now:yyyyMMddHHmmssfff}";

        // Processing ID and version from original
        msh.ProcessingID.ProcessingID.Value = originalMsh.ProcessingID.ProcessingID.Value;
        msh.VersionID.Value = originalMsh.VersionID.Value;
    }

    private static void BuildMsaSegment(
        MSA msa,
        IMessage originalMessage,
        AcknowledgmentCode ackCode,
        string? textMessage)
    {
        var originalMsh = (MSH)originalMessage.GetStructure("MSH");

        // Set acknowledgment code
        msa.AcknowledgementCode.Value = ackCode.ToString();

        // Reference the original message's control ID
        msa.MessageControlID.Value = originalMsh.MessageControlID.Value;

        // Optional text message
        if (!string.IsNullOrEmpty(textMessage))
        {
            msa.TextMessage.Value = textMessage;
        }
    }

    private static void BuildErrSegment(
        ERR err,
        string? segmentId,
        string? fieldPosition,
        string? errorMessage)
    {
        // ERR-1: Error Code and Location
        if (!string.IsNullOrEmpty(segmentId))
        {
            err.GetErrorCodeAndLocation(0).SegmentID.Value = segmentId;

            if (!string.IsNullOrEmpty(fieldPosition))
            {
                err.GetErrorCodeAndLocation(0).FieldPosition.Value = fieldPosition;
            }
        }

        // Note: For more detailed error information, you would populate
        // additional ERR fields based on your error handling requirements
    }
}
