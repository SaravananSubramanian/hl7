using NHapi.Base.Parser;

namespace Com.SaravananSubramanian.Nhapi.Hl7Server;

/// <summary>
/// Routes HL7 messages to appropriate handlers based on message type.
/// Extracts MSH-9 (Message Type) to determine routing.
/// </summary>
public class MessageRouter
{
    private readonly PipeParser _parser = new();

    /// <summary>
    /// Extracts the message type from an HL7 message (MSH-9-1).
    /// </summary>
    /// <param name="rawMessage">The raw HL7 message string</param>
    /// <returns>The message type (e.g., "ADT", "ORM", "ORU")</returns>
    public string GetMessageType(string rawMessage)
    {
        try
        {
            var message = _parser.Parse(rawMessage);
            var terser = new NHapi.Base.Util.Terser(message);
            return terser.Get("MSH-9-1") ?? "UNKNOWN";
        }
        catch
        {
            // Fallback: Parse manually from raw message
            return ParseMessageTypeManually(rawMessage);
        }
    }

    /// <summary>
    /// Extracts the trigger event from an HL7 message (MSH-9-2).
    /// </summary>
    /// <param name="rawMessage">The raw HL7 message string</param>
    /// <returns>The trigger event (e.g., "A01", "O01", "R01")</returns>
    public string GetTriggerEvent(string rawMessage)
    {
        try
        {
            var message = _parser.Parse(rawMessage);
            var terser = new NHapi.Base.Util.Terser(message);
            return terser.Get("MSH-9-2") ?? "UNKNOWN";
        }
        catch
        {
            return ParseTriggerEventManually(rawMessage);
        }
    }

    /// <summary>
    /// Gets the full message type string (e.g., "ADT^A01").
    /// </summary>
    public string GetFullMessageType(string rawMessage)
    {
        return $"{GetMessageType(rawMessage)}^{GetTriggerEvent(rawMessage)}";
    }

    private static string ParseMessageTypeManually(string rawMessage)
    {
        // Find MSH segment and extract field 9
        var lines = rawMessage.Split('\r');
        var mshLine = lines.FirstOrDefault(l => l.StartsWith("MSH"));
        if (mshLine == null) return "UNKNOWN";

        var fields = mshLine.Split('|');
        if (fields.Length < 9) return "UNKNOWN";

        // MSH-9 is the 9th field (index 8), split by ^
        var messageType = fields[8].Split('^');
        return messageType.Length > 0 ? messageType[0] : "UNKNOWN";
    }

    private static string ParseTriggerEventManually(string rawMessage)
    {
        var lines = rawMessage.Split('\r');
        var mshLine = lines.FirstOrDefault(l => l.StartsWith("MSH"));
        if (mshLine == null) return "UNKNOWN";

        var fields = mshLine.Split('|');
        if (fields.Length < 9) return "UNKNOWN";

        var messageType = fields[8].Split('^');
        return messageType.Length > 1 ? messageType[1] : "UNKNOWN";
    }
}
