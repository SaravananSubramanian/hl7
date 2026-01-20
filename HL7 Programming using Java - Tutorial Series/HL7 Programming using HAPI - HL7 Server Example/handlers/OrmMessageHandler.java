package com.saravanansubramanian.hapihl7tutorial.server.handlers;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.protocol.ReceivingApplication;
import ca.uhn.hl7v2.protocol.ReceivingApplicationException;
import ca.uhn.hl7v2.util.Terser;

import java.io.IOException;
import java.time.LocalDateTime;
import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;

/**
 * Handler for ORM (Order) messages.
 * Processes pharmacy, lab, radiology, and other clinical orders.
 *
 * <p>ORM messages are used to transmit orders for various services:
 * <ul>
 *   <li>Laboratory tests</li>
 *   <li>Radiology procedures</li>
 *   <li>Pharmacy prescriptions</li>
 *   <li>Dietary requests</li>
 *   <li>Supply orders</li>
 * </ul>
 *
 * <p>Supported Order Control Codes (ORC-1):
 * <ul>
 *   <li>NW: New Order</li>
 *   <li>CA: Cancel Order</li>
 *   <li>OC: Order Cancelled (notification)</li>
 *   <li>XO: Change Order</li>
 *   <li>HD: Hold Order</li>
 *   <li>RL: Release Hold</li>
 *   <li>SC: Status Changed</li>
 * </ul>
 */
public class OrmMessageHandler implements ReceivingApplication<Message> {

    private static final DateTimeFormatter TIME_FORMAT = DateTimeFormatter.ofPattern("HH:mm:ss");
    private static final DateTimeFormatter FILLER_FORMAT = DateTimeFormatter.ofPattern("yyyyMMddHHmmss");
    private final HapiContext context = new DefaultHapiContext();

    @Override
    public boolean canProcess(Message message) {
        return true;
    }

    @Override
    public Message processMessage(Message message, Map<String, Object> metadata)
            throws ReceivingApplicationException, HL7Exception {

        String timestamp = LocalTime.now().format(TIME_FORMAT);
        System.out.println("\n[" + timestamp + "] ORM Message received:");

        try {
            Terser terser = new Terser(message);

            // Extract order information from ORC segment
            String orderControl = getFieldSafely(terser, "ORC-1", "Unknown");
            String placerOrderNumber = getFieldSafely(terser, "ORC-2-1", "Unknown");
            String fillerOrderNumber = getFieldSafely(terser, "ORC-3-1", "Unknown");
            String orderStatus = getFieldSafely(terser, "ORC-5", "Unknown");

            // Extract test/procedure information from OBR segment
            String universalServiceId = getFieldSafely(terser, "OBR-4-1", "Unknown");
            String serviceName = getFieldSafely(terser, "OBR-4-2", "Unknown");

            // Extract patient info
            String patientId = getFieldSafely(terser, "PID-3-1", "Unknown");
            String lastName = getFieldSafely(terser, "PID-5-1", "");
            String firstName = getFieldSafely(terser, "PID-5-2", "");
            String patientName = lastName + ", " + firstName;

            System.out.println("  ORM Order - Control: " + orderControl + ", Placer#: " + placerOrderNumber);
            System.out.println("    Patient: " + patientName + " (ID: " + patientId + ")");
            System.out.println("    Service: " + serviceName + " (" + universalServiceId + ")");

            // Process based on order control code
            String result = switch (orderControl.toUpperCase()) {
                case "NW" -> processNewOrder(terser, placerOrderNumber, serviceName);
                case "CA" -> processCancelOrder(terser, placerOrderNumber);
                case "XO" -> processChangeOrder(terser, placerOrderNumber);
                case "HD" -> processHoldOrder(terser, placerOrderNumber);
                case "RL" -> processReleaseHold(terser, placerOrderNumber);
                case "SC" -> processStatusChange(terser, placerOrderNumber, orderStatus);
                case "OC" -> processOrderCancelled(terser, placerOrderNumber);
                default -> processGenericOrder(orderControl, placerOrderNumber);
            };

            // Generate a filler order number if this is a new order
            if ("NW".equals(orderControl.toUpperCase())) {
                String generatedFillerNumber = generateFillerOrderNumber();
                System.out.println("    Generated Filler#: " + generatedFillerNumber);
            }

            System.out.println("  Result: " + result);

            // Generate and return ACK
            Message ack = message.generateACK();
            logResponse(ack);
            return ack;

        } catch (IOException e) {
            throw new HL7Exception("Error generating ACK", e);
        }
    }

    private String processNewOrder(Terser terser, String orderNumber, String serviceName) throws HL7Exception {
        String priority = getFieldSafely(terser, "OBR-5", "R"); // Default to Routine
        String scheduledDate = getFieldSafely(terser, "OBR-7-1", "Not specified");
        String orderingProvider = getFieldSafely(terser, "OBR-16-1", "Unknown");

        System.out.println("    -> NEW ORDER: " + serviceName);
        System.out.println("       Priority: " + priority + ", Scheduled: " + scheduledDate);
        System.out.println("       Ordering Provider: " + orderingProvider);

        // In a real system, you would:
        // - Validate the order (insurance, duplicate check, etc.)
        // - Store in order management system
        // - Generate filler order number
        // - Route to appropriate department
        // - Possibly send ORR (Order Response) message

        return "New order " + orderNumber + " accepted";
    }

    private String processCancelOrder(Terser terser, String orderNumber) throws HL7Exception {
        String cancelReason = getFieldSafely(terser, "ORC-16-1", "Not specified");
        System.out.println("    -> CANCEL ORDER: " + orderNumber);
        System.out.println("       Reason: " + cancelReason);

        return "Order " + orderNumber + " cancel request received";
    }

    private String processChangeOrder(Terser terser, String orderNumber) {
        System.out.println("    -> CHANGE ORDER: " + orderNumber);

        return "Order " + orderNumber + " change request received";
    }

    private String processHoldOrder(Terser terser, String orderNumber) {
        System.out.println("    -> HOLD ORDER: " + orderNumber);

        return "Order " + orderNumber + " placed on hold";
    }

    private String processReleaseHold(Terser terser, String orderNumber) {
        System.out.println("    -> RELEASE HOLD: " + orderNumber);

        return "Order " + orderNumber + " hold released";
    }

    private String processStatusChange(Terser terser, String orderNumber, String newStatus) {
        System.out.println("    -> STATUS CHANGE: " + orderNumber + " -> " + newStatus);

        return "Order " + orderNumber + " status updated to " + newStatus;
    }

    private String processOrderCancelled(Terser terser, String orderNumber) {
        System.out.println("    -> ORDER CANCELLED NOTIFICATION: " + orderNumber);

        return "Order cancellation notification received for " + orderNumber;
    }

    private String processGenericOrder(String orderControl, String orderNumber) {
        System.out.println("    -> Generic order processing: " + orderControl);

        return "Order " + orderNumber + " with control " + orderControl + " processed";
    }

    private String generateFillerOrderNumber() {
        String timestamp = LocalDateTime.now().format(FILLER_FORMAT);
        int random = ThreadLocalRandom.current().nextInt(1000, 9999);
        return "FIL" + timestamp + random;
    }

    private String getFieldSafely(Terser terser, String path, String defaultValue) {
        try {
            String value = terser.get(path);
            return value != null && !value.isEmpty() ? value : defaultValue;
        } catch (HL7Exception e) {
            return defaultValue;
        }
    }

    private void logResponse(Message response) throws HL7Exception {
        String timestamp = LocalTime.now().format(TIME_FORMAT);
        String responseStr = context.getPipeParser().encode(response);
        System.out.println("[" + timestamp + "] Response sent:");
        String[] segments = responseStr.split("\r");
        for (int i = 0; i < Math.min(segments.length, 3); i++) {
            System.out.println("  OUT: " + segments[i]);
        }
    }
}
