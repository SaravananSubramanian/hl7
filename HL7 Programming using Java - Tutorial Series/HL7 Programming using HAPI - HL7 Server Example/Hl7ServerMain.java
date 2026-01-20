package com.saravanansubramanian.hapihl7tutorial.server;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.HL7Service;
import ca.uhn.hl7v2.protocol.ApplicationRouter.AppRoutingData;
import com.saravanansubramanian.hapihl7tutorial.server.handlers.*;

import java.util.Scanner;

/**
 * Main entry point for the HL7 V2 server application.
 * Uses HAPI's HL7Service for MLLP-based message reception with message routing.
 *
 * <p><b>Usage:</b> Run with optional port argument (default: 2575)
 * <p><b>Testing:</b> Use the Sending HL7 Messages example or any MLLP client.
 * <p><b>Dependencies:</b> HAPI 2.5+ (hapi-base, hapi-structures-v24)
 *
 * <p>This server demonstrates:
 * <ul>
 *   <li>Using HAPI's built-in HL7Service (SimpleServer)</li>
 *   <li>Message routing based on message type and trigger event</li>
 *   <li>Handling ADT, ORM, ORU, and other message types</li>
 *   <li>Generating appropriate ACK responses</li>
 * </ul>
 *
 * <p>HAPI's HL7Service handles:
 * <ul>
 *   <li>TCP socket management</li>
 *   <li>MLLP framing (SB/EB/CR characters)</li>
 *   <li>Connection handling and threading</li>
 *   <li>Message parsing and routing</li>
 * </ul>
 */
public class Hl7ServerMain {

    private static final int DEFAULT_PORT = 2575; // Standard HL7 port

    public static void main(String[] args) {
        int port = args.length > 0 ? parsePort(args[0]) : DEFAULT_PORT;

        printBanner();

        try (HapiContext context = new DefaultHapiContext()) {
            // Create HL7 server using HAPI's HL7Service
            boolean useTls = false;
            HL7Service server = context.newServer(port, useTls);

            // Register message handlers with routing rules
            registerHandlers(server);

            // Start the server
            server.startAndWait();
            System.out.println("Server listening on port " + port);
            System.out.println("Press Enter to stop the server...");
            System.out.println("-".repeat(50));

            // Wait for user input to stop
            new Scanner(System.in).nextLine();

            // Shutdown
            System.out.println("\nStopping server...");
            server.stopAndWait();
            System.out.println("Server stopped.");

        } catch (Exception e) {
            System.err.println("Server error: " + e.getMessage());
            e.printStackTrace();
        }
    }

    /**
     * Registers message handlers with appropriate routing rules.
     * Each handler processes specific message types and trigger events.
     */
    private static void registerHandlers(HL7Service server) {
        System.out.println("Registered message handlers:");

        // ADT handler - Admission/Discharge/Transfer messages
        server.registerApplication(new AdtRoutingData(), new AdtMessageHandler());
        System.out.println("  - ADT: AdtMessageHandler (A01-A13, all versions)");

        // ORM handler - Order messages
        server.registerApplication(new OrmRoutingData(), new OrmMessageHandler());
        System.out.println("  - ORM: OrmMessageHandler (all trigger events)");

        // ORU handler - Observation Result messages
        server.registerApplication(new OruRoutingData(), new OruMessageHandler());
        System.out.println("  - ORU: OruMessageHandler (R01, etc.)");

        // Generic fallback handler for all other message types
        server.registerApplication(new GenericRoutingData(), new GenericMessageHandler());
        System.out.println("  - *: GenericMessageHandler (fallback for other types)");

        System.out.println();
    }

    private static void printBanner() {
        System.out.println("==============================================");
        System.out.println("  HAPI HL7 V2 Server");
        System.out.println("==============================================");
        System.out.println();
        System.out.println("This server demonstrates handling of:");
        System.out.println("  - ADT (Admission/Discharge/Transfer) messages");
        System.out.println("  - ORM (Order) messages");
        System.out.println("  - ORU (Observation Result) messages");
        System.out.println("  - Generic HL7 V2 messages");
        System.out.println();
    }

    private static int parsePort(String portArg) {
        try {
            return Integer.parseInt(portArg);
        } catch (NumberFormatException e) {
            System.out.println("Invalid port '" + portArg + "', using default: " + DEFAULT_PORT);
            return DEFAULT_PORT;
        }
    }
}

/**
 * Routing data for ADT (Admission/Discharge/Transfer) messages.
 * Matches ADT messages with trigger events A01-A13.
 */
class AdtRoutingData implements AppRoutingData {
    @Override
    public String getMessageType() {
        return "ADT";
    }

    @Override
    public String getTriggerEvent() {
        // Match A01 through A13 using regex
        return "A0[1-9]|A1[0-3]";
    }

    @Override
    public String getProcessingId() {
        return "*"; // Accept all processing IDs (P, D, T)
    }

    @Override
    public String getVersion() {
        return "*"; // Accept all HL7 versions
    }
}

/**
 * Routing data for ORM (Order) messages.
 */
class OrmRoutingData implements AppRoutingData {
    @Override
    public String getMessageType() {
        return "ORM";
    }

    @Override
    public String getTriggerEvent() {
        return "*"; // Accept all trigger events
    }

    @Override
    public String getProcessingId() {
        return "*";
    }

    @Override
    public String getVersion() {
        return "*";
    }
}

/**
 * Routing data for ORU (Observation Result) messages.
 */
class OruRoutingData implements AppRoutingData {
    @Override
    public String getMessageType() {
        return "ORU";
    }

    @Override
    public String getTriggerEvent() {
        return "*"; // Accept R01 and other trigger events
    }

    @Override
    public String getProcessingId() {
        return "*";
    }

    @Override
    public String getVersion() {
        return "*";
    }
}

/**
 * Generic routing data that matches all messages.
 * Acts as a fallback handler for message types not explicitly handled.
 */
class GenericRoutingData implements AppRoutingData {
    @Override
    public String getMessageType() {
        return "*"; // Match all message types
    }

    @Override
    public String getTriggerEvent() {
        return "*";
    }

    @Override
    public String getProcessingId() {
        return "*";
    }

    @Override
    public String getVersion() {
        return "*";
    }
}
