using NHapi.Base.Parser;
using NHapi.Base.Util;
using NHapi.Model.V23.Message;

namespace Com.SaravananSubramanian.Nhapi.Hl7Server;

/// <summary>
/// Handler for ADT (Admission, Discharge, Transfer) messages.
/// Processes patient demographic and visit-related events.
/// </summary>
/// <remarks>
/// Supported ADT Trigger Events:
/// - A01: Admit/Visit Notification
/// - A02: Transfer a Patient
/// - A03: Discharge/End Visit
/// - A04: Register a Patient
/// - A08: Update Patient Information
/// - A11: Cancel Admit/Visit Notification
/// - A12: Cancel Transfer
/// - A13: Cancel Discharge
/// </remarks>
public class AdtMessageHandler : IHl7MessageHandler
{
    private readonly PipeParser _parser = new();
    private readonly MessageRouter _router = new();

    public string HandleMessage(string rawMessage)
    {
        try
        {
            var triggerEvent = _router.GetTriggerEvent(rawMessage);
            var message = _parser.Parse(rawMessage);
            var terser = new Terser(message);

            // Extract patient information for logging/processing
            var patientId = terser.Get("PID-3-1") ?? "Unknown";
            var patientName = $"{terser.Get("PID-5-1")}, {terser.Get("PID-5-2")}";

            Console.WriteLine($"  ADT {triggerEvent} - Patient: {patientName} (ID: {patientId})");

            // Process based on trigger event
            var result = triggerEvent switch
            {
                "A01" => ProcessAdmit(terser),
                "A02" => ProcessTransfer(terser),
                "A03" => ProcessDischarge(terser),
                "A04" => ProcessRegister(terser),
                "A08" => ProcessUpdate(terser),
                "A11" => ProcessCancelAdmit(terser),
                "A12" => ProcessCancelTransfer(terser),
                "A13" => ProcessCancelDischarge(terser),
                _ => ProcessGenericAdt(triggerEvent, terser)
            };

            return AckGenerator.CreateAck(rawMessage, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error processing ADT message: {ex.Message}");
            return AckGenerator.CreateNack(rawMessage, "AE", $"Error processing ADT: {ex.Message}");
        }
    }

    private string ProcessAdmit(Terser terser)
    {
        var location = terser.Get("PV1-3-1") ?? "Unknown";
        var admitType = terser.Get("PV1-4") ?? "Unknown";
        Console.WriteLine($"    -> Admit to location: {location}, Type: {admitType}");

        // In a real system, you would:
        // - Validate patient data
        // - Create/update patient record in database
        // - Trigger downstream workflows (bed management, billing, etc.)

        return "Patient admitted successfully";
    }

    private string ProcessTransfer(Terser terser)
    {
        var priorLocation = terser.Get("PV1-6-1") ?? "Unknown";
        var newLocation = terser.Get("PV1-3-1") ?? "Unknown";
        Console.WriteLine($"    -> Transfer from: {priorLocation} to: {newLocation}");

        return "Patient transferred successfully";
    }

    private string ProcessDischarge(Terser terser)
    {
        var dischargeDate = terser.Get("PV1-45-1") ?? "Unknown";
        var disposition = terser.Get("PV1-36") ?? "Unknown";
        Console.WriteLine($"    -> Discharge date: {dischargeDate}, Disposition: {disposition}");

        return "Patient discharged successfully";
    }

    private string ProcessRegister(Terser terser)
    {
        var visitNumber = terser.Get("PV1-19-1") ?? "Unknown";
        Console.WriteLine($"    -> Registration, Visit #: {visitNumber}");

        return "Patient registered successfully";
    }

    private string ProcessUpdate(Terser terser)
    {
        Console.WriteLine($"    -> Patient information updated");

        return "Patient information updated successfully";
    }

    private string ProcessCancelAdmit(Terser terser)
    {
        Console.WriteLine($"    -> Admit cancelled");
        return "Admit cancelled successfully";
    }

    private string ProcessCancelTransfer(Terser terser)
    {
        Console.WriteLine($"    -> Transfer cancelled");
        return "Transfer cancelled successfully";
    }

    private string ProcessCancelDischarge(Terser terser)
    {
        Console.WriteLine($"    -> Discharge cancelled");
        return "Discharge cancelled successfully";
    }

    private string ProcessGenericAdt(string triggerEvent, Terser terser)
    {
        Console.WriteLine($"    -> Generic ADT processing for {triggerEvent}");
        return $"ADT {triggerEvent} processed successfully";
    }
}
