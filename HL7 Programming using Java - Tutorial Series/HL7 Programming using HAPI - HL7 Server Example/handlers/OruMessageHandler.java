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
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Handler for ORU (Observation Result) messages.
 * Processes laboratory results, radiology reports, and other clinical observations.
 *
 * <p>ORU messages contain clinical observations and results:
 * <ul>
 *   <li>Laboratory test results</li>
 *   <li>Radiology reports</li>
 *   <li>Vital signs</li>
 *   <li>Pathology reports</li>
 *   <li>Microbiology results</li>
 * </ul>
 *
 * <p>Key segments:
 * <ul>
 *   <li>OBR: Observation Request (identifies the test/procedure)</li>
 *   <li>OBX: Observation Result (individual result values)</li>
 * </ul>
 *
 * <p>OBX Value Types (OBX-2):
 * <ul>
 *   <li>NM: Numeric</li>
 *   <li>ST: String</li>
 *   <li>TX: Text</li>
 *   <li>CE: Coded Entry</li>
 *   <li>FT: Formatted Text</li>
 *   <li>ED: Encapsulated Data (for binary attachments)</li>
 * </ul>
 */
public class OruMessageHandler implements ReceivingApplication<Message> {

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
        System.out.println("\n[" + timestamp + "] ORU Message received:");

        try {
            Terser terser = new Terser(message);

            // Extract patient information
            String patientId = getFieldSafely(terser, "PID-3-1", "Unknown");
            String lastName = getFieldSafely(terser, "PID-5-1", "");
            String firstName = getFieldSafely(terser, "PID-5-2", "");
            String patientName = lastName + ", " + firstName;

            // Extract order information from OBR
            String fillerOrderNumber = getFieldSafely(terser, "OBR-3-1", "Unknown");
            String universalServiceId = getFieldSafely(terser, "OBR-4-1", "Unknown");
            String serviceName = getFieldSafely(terser, "OBR-4-2", "Unknown");
            String resultStatus = getFieldSafely(terser, "OBR-25", "Unknown");
            String observationDateTime = getFieldSafely(terser, "OBR-7-1", "Unknown");

            System.out.println("  ORU Result - Order#: " + fillerOrderNumber);
            System.out.println("    Patient: " + patientName + " (ID: " + patientId + ")");
            System.out.println("    Service: " + serviceName + " (" + universalServiceId + ")");
            System.out.println("    Status: " + resultStatus + ", Date: " + observationDateTime);

            // Process OBX segments (results)
            String rawMessage = context.getPipeParser().encode(message);
            processObservations(rawMessage);

            // Determine processing based on result status
            String result = switch (resultStatus) {
                case "F" -> processFinalResult(terser, fillerOrderNumber);
                case "P" -> processPreliminaryResult(terser, fillerOrderNumber);
                case "C" -> processCorrectedResult(terser, fillerOrderNumber);
                case "X" -> processCancelledResult(terser, fillerOrderNumber);
                default -> processGenericResult(terser, fillerOrderNumber, resultStatus);
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

    private void processObservations(String rawMessage) {
        // Parse OBX segments manually for display
        String[] lines = rawMessage.split("\r");
        List<String> obxSegments = new ArrayList<>();
        for (String line : lines) {
            if (line.startsWith("OBX")) {
                obxSegments.add(line);
            }
        }

        if (obxSegments.isEmpty()) {
            System.out.println("    No OBX segments found");
            return;
        }

        System.out.println("    Results (" + obxSegments.size() + " observations):");

        int displayCount = Math.min(obxSegments.size(), 5); // Show first 5 results
        for (int i = 0; i < displayCount; i++) {
            String obx = obxSegments.get(i);
            String[] fields = obx.split("\\|");
            if (fields.length < 6) continue;

            String setId = fields.length > 1 ? fields[1] : "";
            String valueType = fields.length > 2 ? fields[2] : "";
            String observationId = fields.length > 3 ? extractObservationName(fields[3]) : "";
            String value = fields.length > 5 ? fields[5] : "";
            String units = fields.length > 6 ? fields[6] : "";
            String refRange = fields.length > 7 ? fields[7] : "";
            String abnormalFlag = fields.length > 8 ? fields[8] : "";

            String flagDisplay = !abnormalFlag.isEmpty() ? " [" + abnormalFlag + "]" : "";
            String rangeDisplay = !refRange.isEmpty() ? " (ref: " + refRange + ")" : "";

            System.out.println("      " + setId + ". " + observationId + ": " + value + " " + units + rangeDisplay + flagDisplay);
        }

        if (obxSegments.size() > 5) {
            System.out.println("      ... and " + (obxSegments.size() - 5) + " more results");
        }
    }

    private String extractObservationName(String obxField3) {
        // OBX-3 is usually in format: code^name^coding system
        String[] parts = obxField3.split("\\^");
        return parts.length > 1 ? parts[1] : parts[0];
    }

    private String processFinalResult(Terser terser, String orderNumber) {
        System.out.println("    -> FINAL RESULT for order " + orderNumber);

        // In a real system, you would:
        // - Store results in clinical data repository
        // - Update order status to completed
        // - Trigger critical value alerts if applicable
        // - Send notifications to ordering provider
        // - Update patient chart

        return "Final results for order " + orderNumber + " received and processed";
    }

    private String processPreliminaryResult(Terser terser, String orderNumber) {
        System.out.println("    -> PRELIMINARY RESULT for order " + orderNumber);

        return "Preliminary results for order " + orderNumber + " received";
    }

    private String processCorrectedResult(Terser terser, String orderNumber) {
        System.out.println("    -> CORRECTED RESULT for order " + orderNumber);

        // Corrected results require special handling:
        // - Mark previous results as superseded
        // - Notify providers of the correction
        // - Audit trail for the correction

        return "Corrected results for order " + orderNumber + " received";
    }

    private String processCancelledResult(Terser terser, String orderNumber) {
        System.out.println("    -> CANCELLED RESULT for order " + orderNumber);

        return "Result cancellation for order " + orderNumber + " received";
    }

    private String processGenericResult(Terser terser, String orderNumber, String status) {
        System.out.println("    -> Result with status '" + status + "' for order " + orderNumber);

        return "Results (status: " + status + ") for order " + orderNumber + " received";
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
