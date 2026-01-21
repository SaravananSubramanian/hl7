namespace Com.SaravananSubramanian.Nhapi.Hl7Server;

/// <summary>
/// Interface for HL7 message handlers.
/// Implement this interface to create custom handlers for specific message types.
/// </summary>
public interface IHl7MessageHandler
{
    /// <summary>
    /// Processes an incoming HL7 message and returns the response (typically an ACK).
    /// </summary>
    /// <param name="rawMessage">The raw HL7 message string</param>
    /// <returns>The response message (ACK or NAK)</returns>
    string HandleMessage(string rawMessage);
}
