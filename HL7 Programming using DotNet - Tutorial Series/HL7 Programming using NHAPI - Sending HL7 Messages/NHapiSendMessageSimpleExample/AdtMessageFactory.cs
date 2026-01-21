using NHapi.Base.Model;

namespace Com.SaravananSubramanian.Nhapi.SendMessages;

/// <summary>
/// Factory class for creating ADT (Admission, Discharge, Transfer) messages.
/// </summary>
public static class AdtMessageFactory
{
    /// <summary>
    /// Creates an ADT message based on the specified trigger event type.
    /// </summary>
    /// <param name="triggerEvent">The ADT trigger event code (e.g., "A01", "A08")</param>
    /// <returns>A fully constructed HL7 ADT message</returns>
    public static IMessage CreateMessage(string triggerEvent)
    {
        return triggerEvent.ToUpperInvariant() switch
        {
            "A01" => new AdtA01MessageBuilder().Build(),
            _ => throw new ArgumentException(
                $"ADT trigger event '{triggerEvent}' is not supported.",
                nameof(triggerEvent))
        };
    }
}
