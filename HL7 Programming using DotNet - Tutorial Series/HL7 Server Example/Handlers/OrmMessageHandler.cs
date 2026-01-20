using NHapi.Base.Parser;
using NHapi.Base.Util;

namespace NHapiHl7Server;

/// <summary>
/// Handler for ORM (Order) messages.
/// Processes pharmacy, lab, radiology, and other clinical orders.
/// </summary>
/// <remarks>
/// ORM messages are used to transmit orders for various services:
/// - Laboratory tests
/// - Radiology procedures
/// - Pharmacy prescriptions
/// - Dietary requests
/// - Supply orders
///
/// Supported Order Control Codes (ORC-1):
/// - NW: New Order
/// - CA: Cancel Order
/// - OC: Order Cancelled (notification)
/// - XO: Change Order
/// - HD: Hold Order
/// - RL: Release Hold
/// - SC: Status Changed
/// </remarks>
public class OrmMessageHandler : IHl7MessageHandler
{
    private readonly PipeParser _parser = new();
    private readonly MessageRouter _router = new();

    public string HandleMessage(string rawMessage)
    {
        try
        {
            var message = _parser.Parse(rawMessage);
            var terser = new Terser(message);

            // Extract order information
            var orderControl = terser.Get("ORC-1") ?? "Unknown";
            var placerOrderNumber = terser.Get("ORC-2-1") ?? "Unknown";
            var fillerOrderNumber = terser.Get("ORC-3-1") ?? "Unknown";
            var orderStatus = terser.Get("ORC-5") ?? "Unknown";

            // Extract test/procedure information from OBR
            var universalServiceId = terser.Get("OBR-4-1") ?? "Unknown";
            var serviceName = terser.Get("OBR-4-2") ?? "Unknown";

            // Extract patient info
            var patientId = terser.Get("PID-3-1") ?? "Unknown";
            var patientName = $"{terser.Get("PID-5-1")}, {terser.Get("PID-5-2")}";

            Console.WriteLine($"  ORM Order - Control: {orderControl}, Placer#: {placerOrderNumber}");
            Console.WriteLine($"    Patient: {patientName} (ID: {patientId})");
            Console.WriteLine($"    Service: {serviceName} ({universalServiceId})");

            // Process based on order control code
            var result = orderControl.ToUpperInvariant() switch
            {
                "NW" => ProcessNewOrder(terser, placerOrderNumber, serviceName),
                "CA" => ProcessCancelOrder(terser, placerOrderNumber),
                "XO" => ProcessChangeOrder(terser, placerOrderNumber),
                "HD" => ProcessHoldOrder(terser, placerOrderNumber),
                "RL" => ProcessReleaseHold(terser, placerOrderNumber),
                "SC" => ProcessStatusChange(terser, placerOrderNumber, orderStatus),
                "OC" => ProcessOrderCancelled(terser, placerOrderNumber),
                _ => ProcessGenericOrder(orderControl, placerOrderNumber)
            };

            // Generate a filler order number if this is a new order
            if (orderControl == "NW")
            {
                var generatedFillerNumber = GenerateFillerOrderNumber();
                Console.WriteLine($"    Generated Filler#: {generatedFillerNumber}");
            }

            return AckGenerator.CreateAck(rawMessage, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error processing ORM message: {ex.Message}");
            return AckGenerator.CreateNack(rawMessage, "AE", $"Error processing order: {ex.Message}");
        }
    }

    private string ProcessNewOrder(Terser terser, string orderNumber, string serviceName)
    {
        var priority = terser.Get("OBR-5") ?? "R"; // Default to Routine
        var scheduledDate = terser.Get("OBR-7-1") ?? "Not specified";
        var orderingProvider = terser.Get("OBR-16-1") ?? "Unknown";

        Console.WriteLine($"    -> NEW ORDER: {serviceName}");
        Console.WriteLine($"       Priority: {priority}, Scheduled: {scheduledDate}");
        Console.WriteLine($"       Ordering Provider: {orderingProvider}");

        // In a real system, you would:
        // - Validate the order (insurance, duplicate check, etc.)
        // - Store in order management system
        // - Generate filler order number
        // - Route to appropriate department
        // - Possibly send ORR (Order Response) message

        return $"New order {orderNumber} accepted";
    }

    private string ProcessCancelOrder(Terser terser, string orderNumber)
    {
        var cancelReason = terser.Get("ORC-16-1") ?? "Not specified";
        Console.WriteLine($"    -> CANCEL ORDER: {orderNumber}");
        Console.WriteLine($"       Reason: {cancelReason}");

        return $"Order {orderNumber} cancel request received";
    }

    private string ProcessChangeOrder(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> CHANGE ORDER: {orderNumber}");

        return $"Order {orderNumber} change request received";
    }

    private string ProcessHoldOrder(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> HOLD ORDER: {orderNumber}");

        return $"Order {orderNumber} placed on hold";
    }

    private string ProcessReleaseHold(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> RELEASE HOLD: {orderNumber}");

        return $"Order {orderNumber} hold released";
    }

    private string ProcessStatusChange(Terser terser, string orderNumber, string newStatus)
    {
        Console.WriteLine($"    -> STATUS CHANGE: {orderNumber} -> {newStatus}");

        return $"Order {orderNumber} status updated to {newStatus}";
    }

    private string ProcessOrderCancelled(Terser terser, string orderNumber)
    {
        Console.WriteLine($"    -> ORDER CANCELLED NOTIFICATION: {orderNumber}");

        return $"Order cancellation notification received for {orderNumber}";
    }

    private string ProcessGenericOrder(string orderControl, string orderNumber)
    {
        Console.WriteLine($"    -> Generic order processing: {orderControl}");

        return $"Order {orderNumber} with control {orderControl} processed";
    }

    private static string GenerateFillerOrderNumber()
    {
        return $"FIL{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }
}
