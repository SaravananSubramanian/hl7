package com.saravanansubramanian.hapihl7tutorial.ack;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v23.message.ACK;
import ca.uhn.hl7v2.model.v23.segment.ERR;
import ca.uhn.hl7v2.model.v23.segment.MSA;
import ca.uhn.hl7v2.model.v23.segment.MSH;
import ca.uhn.hl7v2.util.Terser;

import java.io.IOException;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.concurrent.ThreadLocalRandom;

/**
 * Builder class for creating ACK (Acknowledgment) messages.
 *
 * <p>This builder creates properly formatted ACK messages in response to
 * incoming HL7 messages. It handles the complex task of:
 * <ul>
 *   <li>Swapping sender/receiver information</li>
 *   <li>Referencing the original message control ID</li>
 *   <li>Setting appropriate acknowledgment codes</li>
 *   <li>Optionally adding error information</li>
 * </ul>
 *
 * <p>Note: In production, consider using HAPI's built-in {@code message.generateACK()}
 * method which handles many of these details automatically.
 */
public class AckMessageBuilder {

    private static final DateTimeFormatter HL7_TIMESTAMP_FORMAT =
            DateTimeFormatter.ofPattern("yyyyMMddHHmmss");

    /**
     * Creates an ACK message in response to the original message.
     *
     * @param originalMessage The original message being acknowledged
     * @param ackCode The acknowledgment code (AA, AE, AR, etc.)
     * @param textMessage Optional text message describing the result
     * @return A fully constructed ACK message
     * @throws HL7Exception if there's an error creating the ACK
     * @throws IOException if there's an I/O error
     */
    public static ACK createAck(Message originalMessage, AcknowledgmentCode ackCode, String textMessage)
            throws HL7Exception, IOException {

        ACK ack = new ACK();

        // Build MSH segment (response header)
        buildMshSegment(ack.getMSH(), originalMessage);

        // Build MSA segment (acknowledgment details)
        buildMsaSegment(ack.getMSA(), originalMessage, ackCode, textMessage);

        return ack;
    }

    /**
     * Creates an ACK message with error details.
     *
     * @param originalMessage The original message being acknowledged
     * @param ackCode The acknowledgment code
     * @param textMessage Text message describing the result
     * @param errorSegmentId Segment where error occurred (e.g., "PID")
     * @param errorFieldPosition Field position of error (e.g., "5")
     * @param errorMessage Detailed error message
     * @return A fully constructed ACK message with error details
     * @throws HL7Exception if there's an error creating the ACK
     * @throws IOException if there's an I/O error
     */
    public static ACK createAckWithError(
            Message originalMessage,
            AcknowledgmentCode ackCode,
            String textMessage,
            String errorSegmentId,
            String errorFieldPosition,
            String errorMessage) throws HL7Exception, IOException {

        ACK ack = createAck(originalMessage, ackCode, textMessage);

        // Add error details
        buildErrSegment(ack.getERR(), errorSegmentId, errorFieldPosition, errorMessage);

        return ack;
    }

    private static void buildMshSegment(MSH msh, Message originalMessage) throws HL7Exception {
        // Use Terser for easy access to original message fields
        Terser terser = new Terser(originalMessage);

        // Standard encoding characters
        msh.getFieldSeparator().setValue("|");
        msh.getEncodingCharacters().setValue("^~\\&");

        // Swap sending and receiving (response goes back to sender)
        msh.getSendingApplication().getNamespaceID().setValue(
                terser.get("/MSH-5-1")); // Original receiving app becomes sending
        msh.getSendingFacility().getNamespaceID().setValue(
                terser.get("/MSH-6-1")); // Original receiving facility becomes sending
        msh.getReceivingApplication().getNamespaceID().setValue(
                terser.get("/MSH-3-1")); // Original sending app becomes receiving
        msh.getReceivingFacility().getNamespaceID().setValue(
                terser.get("/MSH-4-1")); // Original sending facility becomes receiving

        // Set response timestamp
        msh.getDateTimeOfMessage().getTimeOfAnEvent().setValue(getCurrentTimestamp());

        // Message type is ACK with the same trigger event
        msh.getMessageType().getMessageType().setValue("ACK");
        String triggerEvent = terser.get("/MSH-9-2");
        if (triggerEvent != null && !triggerEvent.isEmpty()) {
            msh.getMessageType().getTriggerEvent().setValue(triggerEvent);
        }

        // Generate unique message control ID for the ACK
        msh.getMessageControlID().setValue(generateMessageControlId());

        // Copy processing ID and version from original
        String processingId = terser.get("/MSH-11-1");
        if (processingId != null) {
            msh.getProcessingID().getProcessingID().setValue(processingId);
        }

        String version = terser.get("/MSH-12");
        if (version != null) {
            msh.getVersionID().setValue(version);
        }
    }

    private static void buildMsaSegment(MSA msa, Message originalMessage, AcknowledgmentCode ackCode, String textMessage)
            throws HL7Exception {

        Terser terser = new Terser(originalMessage);

        // Set acknowledgment code
        msa.getAcknowledgementCode().setValue(ackCode.getCode());

        // Reference the original message's control ID
        String originalControlId = terser.get("/MSH-10");
        if (originalControlId != null) {
            msa.getMessageControlID().setValue(originalControlId);
        }

        // Optional text message
        if (textMessage != null && !textMessage.isEmpty()) {
            msa.getTextMessage().setValue(textMessage);
        }
    }

    private static void buildErrSegment(ERR err, String segmentId, String fieldPosition, String errorMessage)
            throws HL7Exception {

        if (segmentId != null && !segmentId.isEmpty()) {
            err.getErrorCodeAndLocation(0).getSegmentID().setValue(segmentId);
        }

        if (fieldPosition != null && !fieldPosition.isEmpty()) {
            err.getErrorCodeAndLocation(0).getFieldPosition().setValue(fieldPosition);
        }

        // Note: Additional error details would be added based on requirements
    }

    private static String getCurrentTimestamp() {
        return LocalDateTime.now().format(HL7_TIMESTAMP_FORMAT);
    }

    private static String generateMessageControlId() {
        int randomSuffix = ThreadLocalRandom.current().nextInt(1000, 9999);
        return "ACK" + getCurrentTimestamp() + randomSuffix;
    }
}
