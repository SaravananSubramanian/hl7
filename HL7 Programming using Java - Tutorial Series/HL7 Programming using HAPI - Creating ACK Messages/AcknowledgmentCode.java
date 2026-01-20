package com.saravanansubramanian.hapihl7tutorial.ack;

/**
 * Enumeration of HL7 acknowledgment codes.
 *
 * <p>These codes are used in the MSA-1 field of ACK messages to indicate
 * the processing status of the original message.
 */
public enum AcknowledgmentCode {

    /**
     * Application Accept - Message processed successfully.
     * The receiving application successfully processed the message.
     */
    AA("Application Accept"),

    /**
     * Application Error - Error in message content.
     * There was a problem with the message content (syntax or semantic error).
     * The sending application should review and potentially resend the message.
     */
    AE("Application Error"),

    /**
     * Application Reject - Message rejected.
     * The receiving application cannot or will not process the message.
     * The message should not be resent without modification.
     */
    AR("Application Reject"),

    /**
     * Commit Accept - Message committed to safe storage.
     * Used in enhanced acknowledgment mode to indicate the message
     * has been committed to persistent storage.
     */
    CA("Commit Accept"),

    /**
     * Commit Error - Commit error.
     * Error occurred while committing the message to storage.
     */
    CE("Commit Error"),

    /**
     * Commit Reject - Commit rejected.
     * The message could not be committed to storage and is rejected.
     */
    CR("Commit Reject");

    private final String description;

    AcknowledgmentCode(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }

    /**
     * Returns the HL7 code string for use in MSA-1.
     */
    public String getCode() {
        return this.name();
    }
}
