package com.saravanansubramanian.hapihl7tutorial.ack;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v23.message.ACK;
import ca.uhn.hl7v2.parser.Parser;

import java.io.IOException;

/**
 * Demonstrates how to create ACK (Acknowledgment) messages in response to incoming HL7 messages.
 *
 * <p>ACK messages are fundamental to HL7 communication as they confirm receipt and processing status.
 *
 * <p>HL7 Acknowledgment Codes:
 * <ul>
 *   <li>AA - Application Accept: Message processed successfully</li>
 *   <li>AE - Application Error: Error in message content (syntax/semantic)</li>
 *   <li>AR - Application Reject: Message rejected (application cannot process)</li>
 *   <li>CA - Commit Accept: Message committed to safe storage</li>
 *   <li>CE - Commit Error: Commit error</li>
 *   <li>CR - Commit Reject: Commit rejected</li>
 * </ul>
 *
 * <p>The ACK message contains:
 * <ul>
 *   <li>MSH - Message Header (mirrors original with response details)</li>
 *   <li>MSA - Message Acknowledgment (acknowledgment code and reference to original)</li>
 *   <li>ERR - Error segment (optional, for error details)</li>
 * </ul>
 */
public class HapiAckMessageExample {

    private static final HapiContext context = new DefaultHapiContext();

    // Sample incoming ADT A01 message to acknowledge
    private static final String INCOMING_MESSAGE =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20240115120000||ADT^A01|MSG001|P|2.3|||AL|NE|\r" +
            "EVN|A01|20240115120000|||\r" +
            "PID|1||12345^^^HOSP^MR||DOE^JOHN^A||19800101|M|||||||||||\r" +
            "PV1|1|I|ICU^101^A|||||||||||||||||||||||||||||||||||||||";

    public static void main(String[] args) {
        System.out.println("=== HAPI ACK Message Generation Example ===\n");

        try {
            Parser pipeParser = context.getPipeParser();

            // Parse the incoming message
            System.out.println("1. Incoming message to acknowledge:");
            System.out.println(formatForDisplay(INCOMING_MESSAGE));
            System.out.println();

            Message parsedMessage = pipeParser.parse(INCOMING_MESSAGE);

            // Demonstrate different ACK scenarios
            demonstrateSuccessfulAck(parsedMessage, pipeParser);
            demonstrateErrorAck(parsedMessage, pipeParser);
            demonstrateRejectAck(parsedMessage, pipeParser);

            // HAPI also provides a built-in way to generate ACKs
            demonstrateHapiBuiltInAck(parsedMessage, pipeParser);

            System.out.println("\n=== ACK Generation Complete ===");

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        } finally {
            try {
                context.close();
            } catch (IOException e) {
                // Ignore cleanup errors
            }
        }
    }

    /**
     * Creates an AA (Application Accept) acknowledgment for successful processing.
     */
    private static void demonstrateSuccessfulAck(Message originalMessage, Parser parser)
            throws HL7Exception, IOException {

        System.out.println("2. Creating AA (Application Accept) ACK:");
        System.out.println("   Use when message was processed successfully.\n");

        ACK ack = AckMessageBuilder.createAck(
                originalMessage,
                AcknowledgmentCode.AA,
                "Message processed successfully");

        System.out.println("   Generated ACK:");
        System.out.println(formatForDisplay(parser.encode(ack)));
        System.out.println();
    }

    /**
     * Creates an AE (Application Error) acknowledgment for processing errors.
     */
    private static void demonstrateErrorAck(Message originalMessage, Parser parser)
            throws HL7Exception, IOException {

        System.out.println("3. Creating AE (Application Error) ACK:");
        System.out.println("   Use when there's an error in message content.\n");

        ACK ack = AckMessageBuilder.createAck(
                originalMessage,
                AcknowledgmentCode.AE,
                "Patient ID 12345 not found in system");

        System.out.println("   Generated ACK:");
        System.out.println(formatForDisplay(parser.encode(ack)));
        System.out.println();
    }

    /**
     * Creates an AR (Application Reject) acknowledgment for rejected messages.
     */
    private static void demonstrateRejectAck(Message originalMessage, Parser parser)
            throws HL7Exception, IOException {

        System.out.println("4. Creating AR (Application Reject) ACK:");
        System.out.println("   Use when message is rejected.\n");

        ACK ack = AckMessageBuilder.createAck(
                originalMessage,
                AcknowledgmentCode.AR,
                "ADT A01 messages are not accepted by this system");

        System.out.println("   Generated ACK:");
        System.out.println(formatForDisplay(parser.encode(ack)));
        System.out.println();
    }

    /**
     * Demonstrates HAPI's built-in method for generating acknowledgments.
     * This is the recommended approach in production code.
     */
    private static void demonstrateHapiBuiltInAck(Message originalMessage, Parser parser)
            throws HL7Exception, IOException {

        System.out.println("5. Using HAPI's built-in generateACK() method:");
        System.out.println("   This is the recommended approach in production.\n");

        // HAPI provides a convenient method to generate ACKs
        Message ack = originalMessage.generateACK();

        System.out.println("   Generated ACK (using built-in method):");
        System.out.println(formatForDisplay(parser.encode(ack)));
    }

    private static String formatForDisplay(String message) {
        return message.replace("\r", "\n   ");
    }
}
