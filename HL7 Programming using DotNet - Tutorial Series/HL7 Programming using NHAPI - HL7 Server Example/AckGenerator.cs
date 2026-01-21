using System.Globalization;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Model.V23.Message;
using NHapi.Model.V23.Segment;

namespace Com.SaravananSubramanian.Nhapi.Hl7Server;

/// <summary>
/// Utility class for generating HL7 ACK (Acknowledgment) messages.
/// Supports both positive (AA) and negative (AE, AR) acknowledgments.
/// </summary>
public static class AckGenerator
{
    private static readonly PipeParser Parser = new();

    /// <summary>
    /// Creates a positive acknowledgment (AA) for the given message.
    /// </summary>
    /// <param name="originalMessage">The original HL7 message being acknowledged</param>
    /// <param name="textMessage">Optional text message</param>
    /// <returns>The ACK message as a string</returns>
    public static string CreateAck(string originalMessage, string? textMessage = null)
    {
        return CreateAckInternal(originalMessage, "AA", textMessage ?? "Message accepted");
    }

    /// <summary>
    /// Creates an acknowledgment with the specified code.
    /// </summary>
    /// <param name="originalMessage">The original HL7 message</param>
    /// <param name="ackCode">The acknowledgment code (AA, AE, AR)</param>
    /// <param name="textMessage">Text message describing the result</param>
    /// <returns>The ACK message as a string</returns>
    public static string CreateAck(string originalMessage, string ackCode, string textMessage)
    {
        return CreateAckInternal(originalMessage, ackCode, textMessage);
    }

    /// <summary>
    /// Creates a negative acknowledgment (Application Error).
    /// </summary>
    public static string CreateNack(string originalMessage, string ackCode, string errorMessage)
    {
        return CreateAckInternal(originalMessage, ackCode, errorMessage);
    }

    private static string CreateAckInternal(string originalMessage, string ackCode, string textMessage)
    {
        try
        {
            var parsedOriginal = Parser.Parse(originalMessage);
            var ack = new ACK();

            BuildMshSegment(ack.MSH, parsedOriginal);
            BuildMsaSegment(ack.MSA, parsedOriginal, ackCode, textMessage);

            return Parser.Encode(ack);
        }
        catch (Exception ex)
        {
            // If parsing fails, create a minimal ACK manually
            return CreateMinimalAck(originalMessage, ackCode, textMessage, ex.Message);
        }
    }

    private static void BuildMshSegment(MSH msh, IMessage originalMessage)
    {
        var originalMsh = (MSH)originalMessage.GetStructure("MSH");

        msh.FieldSeparator.Value = "|";
        msh.EncodingCharacters.Value = "^~\\&";

        // Swap sender/receiver
        msh.SendingApplication.NamespaceID.Value =
            originalMsh.ReceivingApplication.NamespaceID.Value ?? "SERVER";
        msh.SendingFacility.NamespaceID.Value =
            originalMsh.ReceivingFacility.NamespaceID.Value ?? "FACILITY";
        msh.ReceivingApplication.NamespaceID.Value =
            originalMsh.SendingApplication.NamespaceID.Value ?? "CLIENT";
        msh.ReceivingFacility.NamespaceID.Value =
            originalMsh.SendingFacility.NamespaceID.Value ?? "FACILITY";

        msh.DateTimeOfMessage.TimeOfAnEvent.Value =
            DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        msh.MessageType.MessageType.Value = "ACK";
        msh.MessageType.TriggerEvent.Value =
            originalMsh.MessageType.TriggerEvent.Value ?? "";

        msh.MessageControlID.Value = $"ACK{DateTime.Now:yyyyMMddHHmmssfff}";
        msh.ProcessingID.ProcessingID.Value =
            originalMsh.ProcessingID.ProcessingID.Value ?? "P";
        msh.VersionID.Value = originalMsh.VersionID.Value ?? "2.3";
    }

    private static void BuildMsaSegment(MSA msa, IMessage originalMessage, string ackCode, string textMessage)
    {
        var originalMsh = (MSH)originalMessage.GetStructure("MSH");

        msa.AcknowledgementCode.Value = ackCode;
        msa.MessageControlID.Value = originalMsh.MessageControlID.Value ?? "";
        msa.TextMessage.Value = textMessage;
    }

    private static string CreateMinimalAck(string originalMessage, string ackCode, string textMessage, string error)
    {
        // Extract message control ID from original
        var controlId = ExtractControlId(originalMessage);
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        return $"MSH|^~\\&|SERVER|FACILITY|CLIENT|FACILITY|{timestamp}||ACK|ACK{timestamp}|P|2.3\r" +
               $"MSA|{ackCode}|{controlId}|{textMessage}";
    }

    private static string ExtractControlId(string message)
    {
        try
        {
            var lines = message.Split('\r');
            var msh = lines.FirstOrDefault(l => l.StartsWith("MSH"));
            if (msh != null)
            {
                var fields = msh.Split('|');
                if (fields.Length > 9) return fields[9];
            }
        }
        catch { }
        return "UNKNOWN";
    }
}
