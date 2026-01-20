package com.saravanansubramanian.hapihl7tutorial.send;

import com.saravanansubramanian.hapihl7tutorial.create.AdtMessageFactory;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.app.Initiator;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;

/**
 * Demonstrates sending HL7 V2 messages over MLLP using HAPI.
 *
 * <p><b>Prerequisites:</b> An HL7 listener must be running on the target host/port.
 * You can use the HL7 Server Example or any MLLP-compatible HL7 listener.
 *
 * <p><b>Dependencies:</b> Requires hapi-base and hapi-structures-v24 from Maven.
 *
 * @see AdtMessageFactory Factory for creating ADT messages
 */
public class HapiSendMessageSimpleExample {

    // Configuration - adjust for your environment
    private static final String HOST = "localhost";
    private static final int PORT = 52463;
    private static final boolean USE_TLS = false;

    private static final HapiContext context = new DefaultHapiContext();

    public static void main(String[] args) {
        System.out.println("=== HAPI Send Message Example ===\n");
        System.out.println("Target: " + HOST + ":" + PORT + "\n");

        try {
            // Create an ADT A01 message using our factory
            ADT_A01 adtMessage = (ADT_A01) AdtMessageFactory.createMessage("A01");

            // Establish MLLP connection to the remote HL7 listener
            Connection connection = context.newClient(HOST, PORT, USE_TLS);
            Initiator initiator = connection.getInitiator();

            // Display outgoing message
            Parser parser = context.getPipeParser();
            System.out.println("Sending message:\n" + parser.encode(adtMessage));

            // Send message and receive ACK response
            Message response = initiator.sendAndReceive(adtMessage);

            // Display received response
            System.out.println("\nReceived response:\n" + parser.encode(response));

            // Clean up
            connection.close();

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            System.err.println("\nEnsure an HL7 listener is running on " + HOST + ":" + PORT);
            e.printStackTrace();
        } finally {
            try {
                context.close();
            } catch (Exception ignored) {}
        }
    }
}
