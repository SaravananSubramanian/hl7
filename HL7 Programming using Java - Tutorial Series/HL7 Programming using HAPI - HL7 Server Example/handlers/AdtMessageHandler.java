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
 * Handler for ADT (Admission, Discharge, Transfer) messages.
 * Processes patient demographic and visit-related events.
 *
 * <p>Supported ADT Trigger Events:
 * <ul>
 *   <li>A01: Admit/Visit Notification</li>
 *   <li>A02: Transfer a Patient</li>
 *   <li>A03: Discharge/End Visit</li>
 *   <li>A04: Register a Patient</li>
 *   <li>A08: Update Patient Information</li>
 *   <li>A11: Cancel Admit/Visit Notification</li>
 *   <li>A12: Cancel Transfer</li>
 *   <li>A13: Cancel Discharge</li>
 * </ul>
 */
public class AdtMessageHandler implements ReceivingApplication<Message> {

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
        System.out.println("\n[" + timestamp + "] ADT Message received:");

        try {
            Terser terser = new Terser(message);

            // Extract trigger event
            String triggerEvent = terser.get("MSH-9-2");

            // Extract patient information
            String patientId = getFieldSafely(terser, "PID-3-1", "Unknown");
            String lastName = getFieldSafely(terser, "PID-5-1", "");
            String firstName = getFieldSafely(terser, "PID-5-2", "");
            String patientName = lastName + ", " + firstName;

            System.out.println("  ADT " + triggerEvent + " - Patient: " + patientName + " (ID: " + patientId + ")");

            // Process based on trigger event
            String result = switch (triggerEvent) {
                case "A01" -> processAdmit(terser);
                case "A02" -> processTransfer(terser);
                case "A03" -> processDischarge(terser);
                case "A04" -> processRegister(terser);
                case "A08" -> processUpdate(terser);
                case "A11" -> processCancelAdmit(terser);
                case "A12" -> processCancelTransfer(terser);
                case "A13" -> processCancelDischarge(terser);
                default -> processGenericAdt(triggerEvent, terser);
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

    private String processAdmit(Terser terser) throws HL7Exception {
        String location = getFieldSafely(terser, "PV1-3-1", "Unknown");
        String admitType = getFieldSafely(terser, "PV1-4", "Unknown");
        System.out.println("    -> Admit to location: " + location + ", Type: " + admitType);

        // In a real system, you would:
        // - Validate patient data
        // - Create/update patient record in database
        // - Trigger downstream workflows (bed management, billing, etc.)

        return "Patient admitted successfully";
    }

    private String processTransfer(Terser terser) throws HL7Exception {
        String priorLocation = getFieldSafely(terser, "PV1-6-1", "Unknown");
        String newLocation = getFieldSafely(terser, "PV1-3-1", "Unknown");
        System.out.println("    -> Transfer from: " + priorLocation + " to: " + newLocation);

        return "Patient transferred successfully";
    }

    private String processDischarge(Terser terser) throws HL7Exception {
        String dischargeDate = getFieldSafely(terser, "PV1-45-1", "Unknown");
        String disposition = getFieldSafely(terser, "PV1-36", "Unknown");
        System.out.println("    -> Discharge date: " + dischargeDate + ", Disposition: " + disposition);

        return "Patient discharged successfully";
    }

    private String processRegister(Terser terser) throws HL7Exception {
        String visitNumber = getFieldSafely(terser, "PV1-19-1", "Unknown");
        System.out.println("    -> Registration, Visit #: " + visitNumber);

        return "Patient registered successfully";
    }

    private String processUpdate(Terser terser) {
        System.out.println("    -> Patient information updated");

        return "Patient information updated successfully";
    }

    private String processCancelAdmit(Terser terser) {
        System.out.println("    -> Admit cancelled");
        return "Admit cancelled successfully";
    }

    private String processCancelTransfer(Terser terser) {
        System.out.println("    -> Transfer cancelled");
        return "Transfer cancelled successfully";
    }

    private String processCancelDischarge(Terser terser) {
        System.out.println("    -> Discharge cancelled");
        return "Discharge cancelled successfully";
    }

    private String processGenericAdt(String triggerEvent, Terser terser) {
        System.out.println("    -> Generic ADT processing for " + triggerEvent);
        return "ADT " + triggerEvent + " processed successfully";
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
