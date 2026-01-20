using NHapi.Base.Parser;
using NHapi.Base.Util;

namespace NHapiHl7Server;

/// <summary>
/// Generic fallback handler for HL7 messages that don't have a specific handler.
/// Provides basic processing and acknowledgment for any HL7 V2 message type.
/// </summary>
/// <remarks>
/// This handler is used when:
/// - No specific handler is registered for the message type
/// - As a fallback when specific handlers fail
/// - For message types that only need basic acknowledgment
///
/// Supports any valid HL7 V2 message by:
/// - Parsing using NHAPI's generic parser
/// - Extracting key identification fields
/// - Generating appropriate acknowledgment
/// </remarks>
public class GenericMessageHandler : IHl7MessageHandler
{
    private readonly PipeParser _parser = new();
    private readonly MessageRouter _router = new();

    public string HandleMessage(string rawMessage)
    {
        try
        {
            var messageType = _router.GetMessageType(rawMessage);
            var triggerEvent = _router.GetTriggerEvent(rawMessage);

            var message = _parser.Parse(rawMessage);
            var terser = new Terser(message);

            // Extract common fields
            var controlId = terser.Get("MSH-10") ?? "Unknown";
            var sendingApp = terser.Get("MSH-3-1") ?? "Unknown";
            var sendingFacility = terser.Get("MSH-4-1") ?? "Unknown";
            var version = terser.Get("MSH-12") ?? "Unknown";

            Console.WriteLine($"  Generic Handler - {messageType}^{triggerEvent}");
            Console.WriteLine($"    From: {sendingApp} @ {sendingFacility}");
            Console.WriteLine($"    Control ID: {controlId}, Version: {version}");

            // Try to extract patient info if available
            var patientId = terser.Get("PID-3-1");
            if (!string.IsNullOrEmpty(patientId))
            {
                var patientName = $"{terser.Get("PID-5-1")}, {terser.Get("PID-5-2")}";
                Console.WriteLine($"    Patient: {patientName} (ID: {patientId})");
            }

            // Log segment count
            var segments = rawMessage.Split('\r', StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"    Segments: {segments.Length}");

            // Process based on general message category
            var result = messageType switch
            {
                "QRY" or "QBP" => ProcessQuery(terser, messageType, triggerEvent),
                "MFN" => ProcessMasterFile(terser, triggerEvent),
                "SIU" => ProcessScheduling(terser, triggerEvent),
                "RDE" or "RDS" or "RGV" => ProcessPharmacy(terser, messageType, triggerEvent),
                "MDM" => ProcessDocument(terser, triggerEvent),
                "DFT" => ProcessFinancial(terser, triggerEvent),
                _ => ProcessUnknown(messageType, triggerEvent)
            };

            return AckGenerator.CreateAck(rawMessage, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error in generic handler: {ex.Message}");
            return AckGenerator.CreateNack(rawMessage, "AE", $"Error processing message: {ex.Message}");
        }
    }

    private string ProcessQuery(Terser terser, string messageType, string triggerEvent)
    {
        var queryId = terser.Get("QPD-1") ?? terser.Get("QRD-1") ?? "Unknown";
        Console.WriteLine($"    -> Query message: {queryId}");

        // In a real system, you would:
        // - Parse the query parameters
        // - Execute the query against your database
        // - Return results in appropriate response message (RSP, etc.)

        return $"Query {messageType}^{triggerEvent} received - response would be sent separately";
    }

    private string ProcessMasterFile(Terser terser, string triggerEvent)
    {
        var eventType = terser.Get("MFI-1") ?? "Unknown";
        Console.WriteLine($"    -> Master File update: {eventType}");

        // Master file messages update reference data like:
        // - Staff/practitioner records
        // - Location definitions
        // - Charge descriptions
        // - Test/procedure catalogs

        return $"Master File update ({triggerEvent}) processed";
    }

    private string ProcessScheduling(Terser terser, string triggerEvent)
    {
        var appointmentId = terser.Get("SCH-1-1") ?? "Unknown";
        var appointmentType = terser.Get("SCH-7-1") ?? "Unknown";
        Console.WriteLine($"    -> Scheduling message: Appt {appointmentId}, Type: {appointmentType}");

        return $"Scheduling message ({triggerEvent}) processed";
    }

    private string ProcessPharmacy(Terser terser, string messageType, string triggerEvent)
    {
        var rxNumber = terser.Get("RXE-15") ?? terser.Get("RXO-1-1") ?? "Unknown";
        Console.WriteLine($"    -> Pharmacy message: Rx {rxNumber}");

        return $"Pharmacy message {messageType}^{triggerEvent} processed";
    }

    private string ProcessDocument(Terser terser, string triggerEvent)
    {
        var docType = terser.Get("TXA-2") ?? "Unknown";
        var docId = terser.Get("TXA-12-1") ?? "Unknown";
        Console.WriteLine($"    -> Document message: Type {docType}, ID {docId}");

        return $"Document message ({triggerEvent}) processed";
    }

    private string ProcessFinancial(Terser terser, string triggerEvent)
    {
        var transactionType = terser.Get("FT1-6") ?? "Unknown";
        Console.WriteLine($"    -> Financial transaction: {transactionType}");

        return $"Financial message ({triggerEvent}) processed";
    }

    private string ProcessUnknown(string messageType, string triggerEvent)
    {
        Console.WriteLine($"    -> Unknown message type processed generically");

        return $"Message {messageType}^{triggerEvent} received and acknowledged";
    }
}
