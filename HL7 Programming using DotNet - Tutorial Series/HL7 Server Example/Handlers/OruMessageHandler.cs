using NHapi.Base.Parser;
using NHapi.Base.Util;

namespace NHapiHl7Server;

/// <summary>
/// Handler for ORU (Observation Result) messages.
/// Processes laboratory results, radiology reports, and other clinical observations.
/// </summary>
/// <remarks>
/// ORU messages contain clinical observations and results:
/// - Laboratory test results
/// - Radiology reports
/// - Vital signs
/// - Pathology reports
/// - Microbiology results
///
/// Key segments:
/// - OBR: Observation Request (identifies the test/procedure)
/// - OBX: Observation Result (individual result values)
///
/// OBX Value Types (OBX-2):
/// - NM: Numeric
/// - ST: String
/// - TX: Text
/// - CE: Coded Entry
/// - FT: Formatted Text
/// - ED: Encapsulated Data (for binary attachments)
/// </remarks>
public class OruMessageHandler : IHl7MessageHandler
{
    private readonly PipeParser _parser = new();

    public string HandleMessage(string rawMessage)
    {
        try
        {
            var message = _parser.Parse(rawMessage);
            var terser = new Terser(message);

            // Extract patient information
            var patientId = terser.Get("PID-3-1") ?? "Unknown";
            var patientName = $"{terser.Get("PID-5-1")}, {terser.Get("PID-5-2")}";

            // Extract order information from OBR
            var fillerOrderNumber = terser.Get("OBR-3-1") ?? "Unknown";
            var universalServiceId = terser.Get("OBR-4-1") ?? "Unknown";
            var serviceName = terser.Get("OBR-4-2") ?? "Unknown";
            var resultStatus = terser.Get("OBR-25") ?? "Unknown";
            var observationDateTime = terser.Get("OBR-7-1") ?? "Unknown";

            Console.WriteLine($"  ORU Result - Order#: {fillerOrderNumber}");
            Console.WriteLine($"    Patient: {patientName} (ID: {patientId})");
            Console.WriteLine($"    Service: {serviceName} ({universalServiceId})");
            Console.WriteLine($"    Status: {resultStatus}, Date: {observationDateTime}");

            // Process OBX segments (results)
            ProcessObservations(rawMessage);

            // Determine processing based on result status
            var processingResult = resultStatus switch
            {
                "F" => ProcessFinalResult(terser, fillerOrderNumber),
                "P" => ProcessPreliminaryResult(terser, fillerOrderNumber),
                "C" => ProcessCorrectedResult(terser, fillerOrderNumber),
                "X" => ProcessCancelledResult(terser, fillerOrderNumber),
                _ => ProcessGenericResult(terser, fillerOrderNumber, resultStatus)
            };

            return AckGenerator.CreateAck(rawMessage, processingResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error processing ORU message: {ex.Message}");
            return AckGenerator.CreateNack(rawMessage, "AE", $"Error processing results: {ex.Message}");
        }
    }

    private void ProcessObservations(string rawMessage)
    {
        // Parse OBX segments manually for display
        var lines = rawMessage.Split('\r');
        var obxSegments = lines.Where(l => l.StartsWith("OBX")).ToList();

        if (obxSegments.Count == 0)
        {
            Console.WriteLine($"    No OBX segments found");
            return;
        }

        Console.WriteLine($"    Results ({obxSegments.Count} observations):");

        foreach (var obx in obxSegments.Take(5)) // Show first 5 results
        {
            var fields = obx.Split('|');
            if (fields.Length < 6) continue;

            var setId = fields.Length > 1 ? fields[1] : "";
            var valueType = fields.Length > 2 ? fields[2] : "";
            var observationId = fields.Length > 3 ? fields[3].Split('^').ElementAtOrDefault(1) ?? fields[3] : "";
            var value = fields.Length > 5 ? fields[5] : "";
            var units = fields.Length > 6 ? fields[6] : "";
            var refRange = fields.Length > 7 ? fields[7] : "";
            var abnormalFlag = fields.Length > 8 ? fields[8] : "";
            var status = fields.Length > 11 ? fields[11] : "";

            var flagDisplay = !string.IsNullOrEmpty(abnormalFlag) ? $" [{abnormalFlag}]" : "";
            var rangeDisplay = !string.IsNullOrEmpty(refRange) ? $" (ref: {refRange})" : "";

            Console.WriteLine($"      {setId}. {observationId}: {value} {units}{rangeDisplay}{flagDisplay}");
        }

        if (obxSegments.Count > 5)
        {
            Console.WriteLine($"      ... and {obxSegments.Count - 5} more results");
        }
    }

    private string ProcessFinalResult(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> FINAL RESULT for order {orderNumber}");

        // In a real system, you would:
        // - Store results in clinical data repository
        // - Update order status to completed
        // - Trigger critical value alerts if applicable
        // - Send notifications to ordering provider
        // - Update patient chart

        return $"Final results for order {orderNumber} received and processed";
    }

    private string ProcessPreliminaryResult(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> PRELIMINARY RESULT for order {orderNumber}");

        return $"Preliminary results for order {orderNumber} received";
    }

    private string ProcessCorrectedResult(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> CORRECTED RESULT for order {orderNumber}");

        // Corrected results require special handling:
        // - Mark previous results as superseded
        // - Notify providers of the correction
        // - Audit trail for the correction

        return $"Corrected results for order {orderNumber} received";
    }

    private string ProcessCancelledResult(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> CANCELLED RESULT for order {orderNumber}");

        return $"Result cancellation for order {orderNumber} received";
    }

    private string ProcessGenericResult(Terser terser, string orderNumber, string status)
    {
        Console.WriteLine($"    -> Result with status '{status}' for order {orderNumber}");

        return $"Results (status: {status}) for order {orderNumber} received";
    }
}
