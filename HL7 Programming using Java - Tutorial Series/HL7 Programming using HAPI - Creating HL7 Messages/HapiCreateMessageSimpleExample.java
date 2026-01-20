package com.saravanansubramanian.hapihl7tutorial.create;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;

/**
 * Demonstrates how to create HL7 V2 messages using HAPI and serialize them
 * to both pipe-delimited and XML formats.
 *
 * <p>This example shows:
 * <ul>
 *   <li>Using HapiContext as the central configuration object</li>
 *   <li>Creating ADT A01 messages programmatically</li>
 *   <li>Encoding messages to pipe-delimited format</li>
 *   <li>Encoding messages to XML format</li>
 *   <li>Writing messages to files</li>
 * </ul>
 *
 * @see AdtMessageFactory Factory for creating different ADT message types
 * @see OurAdtA01MessageBuilder Builder for ADT A01 message construction
 */
public class HapiCreateMessageSimpleExample {

    // HapiContext is the central configuration object in HAPI
    // It manages parsers, validators, and other settings
    private static final HapiContext context = new DefaultHapiContext();

    public static void main(String[] args) {
        System.out.println("=== HAPI Create Message Example ===\n");

        try {
            createAndSerializeMessage();
        } catch (Exception e) {
            System.err.println("Error creating HL7 message: " + e.getMessage());
            e.printStackTrace();
        } finally {
            // Clean up context resources
            try {
                context.close();
            } catch (IOException e) {
                // Ignore cleanup errors
            }
        }
    }

    /**
     * Creates an ADT A01 message and serializes it to both pipe-delimited and XML formats.
     */
    private static void createAndSerializeMessage() throws HL7Exception, IOException {
        // Create the ADT A01 message using our factory
        System.out.println("1. Creating ADT A01 message...");
        ADT_A01 adtMessage = (ADT_A01) AdtMessageFactory.createMessage("A01");

        // Get parsers from the context
        // PipeParser: Traditional HL7 pipe-delimited format (most common)
        // XMLParser: HL7 XML encoding format
        Parser pipeParser = context.getPipeParser();
        Parser xmlParser = context.getXMLParser();

        // Display the pipe-delimited message
        System.out.println("\n2. Pipe-delimited format:");
        String pipeMessage = pipeParser.encode(adtMessage);
        System.out.println(formatForDisplay(pipeMessage));

        // Write to files
        System.out.println("\n3. Writing to files...");
        writeMessageToFile(pipeParser, adtMessage, "testPipeDelimitedOutputFile.txt");
        writeMessageToFile(xmlParser, adtMessage, "testXmlOutputFile.xml");

        // Display message structure (useful for debugging)
        System.out.println("\n4. Message structure:");
        System.out.println(adtMessage.printStructure());

        System.out.println("\n=== Message creation complete ===");
    }

    /**
     * Writes an HL7 message to a file using the specified parser encoding.
     *
     * @param parser The parser to use for encoding
     * @param adtMessage The message to serialize
     * @param outputFilename The output file name
     */
    private static void writeMessageToFile(Parser parser, ADT_A01 adtMessage, String outputFilename)
            throws IOException, HL7Exception {

        Path filePath = Path.of(outputFilename);
        String encodedMessage = parser.encode(adtMessage);

        // Use try-with-resources for proper file handling (Java 7+)
        Files.writeString(filePath, encodedMessage, StandardCharsets.UTF_8);

        System.out.println("   Wrote " + encodedMessage.length() + " characters to " + filePath.toAbsolutePath());
    }

    /**
     * Formats an HL7 message for console display by adding line breaks.
     */
    private static String formatForDisplay(String message) {
        // Replace carriage returns with newlines for display
        return message.replace("\r", "\n   ");
    }
}
