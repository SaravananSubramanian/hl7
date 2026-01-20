package com.saravanansubramanian.hapihl7tutorial.server.handlers;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.protocol.ReceivingApplication;
import ca.uhn.hl7v2.protocol.ReceivingApplicationException;
import ca.uhn.hl7v2.util.Terser;

import java.io.IOException;
import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.util.Map;

/**
 * Generic fallback handler for HL7 messages that don't have a specific handler.
 * Provides basic processing and acknowledgment for any HL7 V2 message type.
 *
 * <p>This handler is used when:
 * <ul>
 *   <li>No specific handler is registered for the message type</li>
 *   <li>As a fallback when specific handlers fail</li>
 *   <li>For message types that only need basic acknowledgment</li>
 * </ul>
 *
 * <p>Supports any valid HL7 V2 message by:
 * <ul>
 *   <li>Parsing using HAPI's generic parser</li>
 *   <li>Extracting key identification fields</li>
 *   <li>Generating appropriate acknowledgment</li>
 * </ul>
 */
public class GenericMessageHandler implements ReceivingApplication<Message> {

    private static final DateTimeFormatter TIME_FORMAT = DateTimeFormatter.ofPattern("HH:mm:ss");
    private final HapiContext context = new DefaultHapiContext();

    @Override
    public boolean canProcess(Message message) {
        return true;
    }

    @Override
    public Message processMessage(Message message, Map<String, Object> metadata)
            throws ReceivingApplicationException, HL7Exception {

        String timestamp = LocalTime.now().format(TIME_FORMAT);
        System.out.println("\n[" + timestamp + "] Message received:");

        try {
            Terser terser = new Terser(message);

            // Extract message identification
            String messageType = getFieldSafely(terser, "MSH-9-1", "Unknown");
            String triggerEvent = getFieldSafely(terser, "MSH-9-2", "Unknown");
            String controlId = getFieldSafely(terser, "MSH-10", "Unknown");
            String sendingApp = getFieldSafely(terser, "MSH-3-1", "Unknown");
            String sendingFacility = getFieldSafely(terser, "MSH-4-1", "Unknown");
            String version = getFieldSafely(terser, "MSH-12", "Unknown");

            System.out.println("  Generic Handler - " + messageType + "^" + triggerEvent);
            System.out.println("    From: " + sendingApp + " @ " + sendingFacility);
            System.out.println("    Control ID: " + controlId + ", Version: " + version);

            // Try to extract patient info if available
            String patientId = getFieldSafely(terser, "PID-3-1", null);
            if (patientId != null) {
                String lastName = getFieldSafely(terser, "PID-5-1", "");
                String firstName = getFieldSafely(terser, "PID-5-2", "");
                String patientName = lastName + ", " + firstName;
                System.out.println("    Patient: " + patientName + " (ID: " + patientId + ")");
            }

            // Log segment count
            String rawMessage = context.getPipeParser().encode(message);
            String[] segments = rawMessage.split("\r");
            System.out.println("    Segments: " + segments.length);

            // Process based on general message category
            String result = switch (messageType) {
                case "QRY", "QBP" -> processQuery(terser, messageType, triggerEvent);
                case "MFN" -> processMasterFile(terser, triggerEvent);
                case "SIU" -> processScheduling(terser, triggerEvent);
                case "RDE", "RDS", "RGV" -> processPharmacy(terser, messageType, triggerEvent);
                case "MDM" -> processDocument(terser, triggerEvent);
                case "DFT" -> processFinancial(terser, triggerEvent);
                default -> processUnknown(messageType, triggerEvent);
            };

            System.out.println("  Result: " + result);

            // Generate and return ACK
            Message ack = message.generateACK();
            logResponse(ack);
            return ack;

        } catch (IOException e) {
            throw new HL7Exception("Error generating ACK", e);
        }
    }

    private String processQuery(Terser terser, String messageType, String triggerEvent) throws HL7Exception {
        String queryId = getFieldSafely(terser, "QPD-1", null);
        if (queryId == null) {
            queryId = getFieldSafely(terser, "QRD-1", "Unknown");
        }
        System.out.println("    -> Query message: " + queryId);

        // In a real system, you would:
        // - Parse the query parameters
        // - Execute the query against your database
        // - Return results in appropriate response message (RSP, etc.)

        return "Query " + messageType + "^" + triggerEvent + " received - response would be sent separately";
    }

    private String processMasterFile(Terser terser, String triggerEvent) throws HL7Exception {
        String eventType = getFieldSafely(terser, "MFI-1", "Unknown");
        System.out.println("    -> Master File update: " + eventType);

        // Master file messages update reference data like:
        // - Staff/practitioner records
        // - Location definitions
        // - Charge descriptions
        // - Test/procedure catalogs

        return "Master File update (" + triggerEvent + ") processed";
    }

    private String processScheduling(Terser terser, String triggerEvent) throws HL7Exception {
        String appointmentId = getFieldSafely(terser, "SCH-1-1", "Unknown");
        String appointmentType = getFieldSafely(terser, "SCH-7-1", "Unknown");
        System.out.println("    -> Scheduling message: Appt " + appointmentId + ", Type: " + appointmentType);

        return "Scheduling message (" + triggerEvent + ") processed";
    }

    private String processPharmacy(Terser terser, String messageType, String triggerEvent) throws HL7Exception {
        String rxNumber = getFieldSafely(terser, "RXE-15", null);
        if (rxNumber == null) {
            rxNumber = getFieldSafely(terser, "RXO-1-1", "Unknown");
        }
        System.out.println("    -> Pharmacy message: Rx " + rxNumber);

        return "Pharmacy message " + messageType + "^" + triggerEvent + " processed";
    }

    private String processDocument(Terser terser, String triggerEvent) throws HL7Exception {
        String docType = getFieldSafely(terser, "TXA-2", "Unknown");
        String docId = getFieldSafely(terser, "TXA-12-1", "Unknown");
        System.out.println("    -> Document message: Type " + docType + ", ID " + docId);

        return "Document message (" + triggerEvent + ") processed";
    }

    private String processFinancial(Terser terser, String triggerEvent) throws HL7Exception {
        String transactionType = getFieldSafely(terser, "FT1-6", "Unknown");
        System.out.println("    -> Financial transaction: " + transactionType);

        return "Financial message (" + triggerEvent + ") processed";
    }

    private String processUnknown(String messageType, String triggerEvent) {
        System.out.println("    -> Unknown message type processed generically");

        return "Message " + messageType + "^" + triggerEvent + " received and acknowledged";
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
