package com.saravanansubramanian.hapihl7tutorial.create;

import java.io.IOException;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.Message;

/**
 * Factory class for creating ADT (Admission, Discharge, Transfer) messages.
 *
 * <p>ADT messages are one of the most common HL7 message types, used to communicate
 * patient demographic and visit information between healthcare systems.
 *
 * <p>Common ADT trigger events:
 * <ul>
 *   <li>A01 - Admit/Visit Notification</li>
 *   <li>A02 - Transfer a Patient</li>
 *   <li>A03 - Discharge/End Visit</li>
 *   <li>A04 - Register a Patient</li>
 *   <li>A08 - Update Patient Information</li>
 *   <li>A11 - Cancel Admit/Visit Notification</li>
 * </ul>
 */
public class AdtMessageFactory {

    /**
     * Creates an ADT message based on the specified trigger event type.
     *
     * @param messageType The ADT trigger event code (e.g., "A01", "A08")
     * @return A fully constructed HL7 ADT message
     * @throws IllegalArgumentException if the message type is not supported
     * @throws HL7Exception if there's an error creating the message
     * @throws IOException if there's an I/O error during message creation
     */
    public static Message createMessage(String messageType) throws HL7Exception, IOException {
        // Normalize the message type for comparison
        String normalizedType = messageType.toUpperCase().trim();

        // Use switch expression (Java 14+) for cleaner code
        if ("A01".equals(normalizedType)) {
            return new OurAdtA01MessageBuilder().Build();
        }

        // Add additional ADT message types here as needed:
        // case "A02" -> new AdtA02MessageBuilder().Build();
        // case "A03" -> new AdtA03MessageBuilder().Build();
        // case "A04" -> new AdtA04MessageBuilder().Build();
        // case "A08" -> new AdtA08MessageBuilder().Build();

        throw new IllegalArgumentException(
                String.format("ADT trigger event '%s' is not supported. " +
                        "Supported events: A01. Extend this factory to add more.", messageType));
    }
}
