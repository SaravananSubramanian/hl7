using NHapi.Base.Model;

namespace NHapiCreateMessageSimpleExample;

/// <summary>
/// Factory class for creating ADT (Admission, Discharge, Transfer) messages.
/// Uses the Factory pattern to abstract message creation and allow easy extension.
/// </summary>
/// <remarks>
/// ADT messages are one of the most common HL7 message types, used to communicate
/// patient demographic and visit information between healthcare systems.
///
/// Common ADT trigger events:
/// - A01: Admit/Visit Notification
/// - A02: Transfer a Patient
/// - A03: Discharge/End Visit
/// - A04: Register a Patient
/// - A08: Update Patient Information
/// - A11: Cancel Admit/Visit Notification
/// </remarks>
public static class AdtMessageFactory
{
    /// <summary>
    /// Creates an ADT message based on the specified trigger event type.
    /// </summary>
    /// <param name="triggerEvent">The ADT trigger event code (e.g., "A01", "A08")</param>
    /// <returns>A fully constructed HL7 ADT message</returns>
    /// <exception cref="ArgumentException">Thrown when the trigger event is not supported</exception>
    public static IMessage CreateMessage(string triggerEvent)
    {
        // Use pattern matching to route to the appropriate builder
        // This pattern allows easy extension for additional ADT message types
        return triggerEvent.ToUpperInvariant() switch
        {
            "A01" => new AdtA01MessageBuilder().Build(),

            // Add additional ADT message types here as needed:
            // "A02" => new AdtA02MessageBuilder().Build(),
            // "A03" => new AdtA03MessageBuilder().Build(),
            // "A04" => new AdtA04MessageBuilder().Build(),
            // "A08" => new AdtA08MessageBuilder().Build(),

            _ => throw new ArgumentException(
                $"ADT trigger event '{triggerEvent}' is not supported. " +
                $"Supported events: A01. Extend this factory to add more.",
                nameof(triggerEvent))
        };
    }
}
