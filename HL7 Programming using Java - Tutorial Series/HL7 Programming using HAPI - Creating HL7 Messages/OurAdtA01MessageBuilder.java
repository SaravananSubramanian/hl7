package com.saravanansubramanian.hapihl7tutorial.create;

import java.io.IOException;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.concurrent.ThreadLocalRandom;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.DataTypeException;
import ca.uhn.hl7v2.model.v24.datatype.PL;
import ca.uhn.hl7v2.model.v24.datatype.XAD;
import ca.uhn.hl7v2.model.v24.datatype.XCN;
import ca.uhn.hl7v2.model.v24.datatype.XPN;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.model.v24.segment.EVN;
import ca.uhn.hl7v2.model.v24.segment.MSH;
import ca.uhn.hl7v2.model.v24.segment.PID;
import ca.uhn.hl7v2.model.v24.segment.PV1;

/**
 * Builder class for constructing ADT A01 (Admit/Visit Notification) messages.
 *
 * <p>An ADT A01 message is sent when a patient is admitted as an inpatient or
 * registered as an outpatient. It typically contains:
 * <ul>
 *   <li>MSH - Message Header (routing and identification)</li>
 *   <li>EVN - Event Type (trigger event information)</li>
 *   <li>PID - Patient Identification (demographics)</li>
 *   <li>PV1 - Patient Visit (visit/encounter information)</li>
 * </ul>
 *
 * <p>This example uses HL7 v2.4 message structures.
 *
 * <p>In a real integration, you would pass a domain object or DTO containing
 * the actual patient and visit data from your system.
 */
public class OurAdtA01MessageBuilder {

    private static final DateTimeFormatter HL7_TIMESTAMP_FORMAT =
            DateTimeFormatter.ofPattern("yyyyMMddHHmmss");

    private ADT_A01 adtMessage;

    /**
     * Builds a complete ADT A01 message with sample patient data.
     *
     * @return A fully populated ADT A01 message
     * @throws HL7Exception if there's an error building the message
     * @throws IOException if there's an I/O error
     */
    public ADT_A01 Build() throws HL7Exception, IOException {
        String currentTimestamp = getCurrentTimestamp();
        adtMessage = new ADT_A01();

        // Initialize with quick defaults (sets MSH-9 message type)
        adtMessage.initQuickstart("ADT", "A01", "P");

        // Build each segment
        createMshSegment(currentTimestamp);
        createEvnSegment(currentTimestamp);
        createPidSegment();
        createPv1Segment();

        return adtMessage;
    }

    /**
     * Constructs the MSH (Message Header) segment.
     * Defines message structure, encoding, and routing information.
     */
    private void createMshSegment(String currentTimestamp) throws DataTypeException {
        MSH msh = adtMessage.getMSH();

        // Encoding characters define how the message is parsed
        msh.getFieldSeparator().setValue("|");
        msh.getEncodingCharacters().setValue("^~\\&");

        // Sending and receiving application/facility
        msh.getSendingApplication().getNamespaceID().setValue("Our System");
        msh.getSendingFacility().getNamespaceID().setValue("Our Facility");
        msh.getReceivingApplication().getNamespaceID().setValue("Their Remote System");
        msh.getReceivingFacility().getNamespaceID().setValue("Their Remote Facility");

        // Timestamp and control ID
        msh.getDateTimeOfMessage().getTimeOfAnEvent().setValue(currentTimestamp);
        msh.getMessageControlID().setValue(generateMessageControlId());

        // Version (initQuickstart sets message type)
        msh.getVersionID().getVersionID().setValue("2.4");
    }

    /**
     * Constructs the EVN (Event Type) segment.
     * Provides information about the triggering event.
     */
    private void createEvnSegment(String currentTimestamp) throws DataTypeException {
        EVN evn = adtMessage.getEVN();
        evn.getEventTypeCode().setValue("A01");
        evn.getRecordedDateTime().getTimeOfAnEvent().setValue(currentTimestamp);
    }

    /**
     * Constructs the PID (Patient Identification) segment.
     * Contains patient demographic information.
     */
    private void createPidSegment() throws DataTypeException {
        PID pid = adtMessage.getPID();

        // Patient name (XPN - Extended Person Name)
        XPN patientName = pid.getPatientName(0);
        patientName.getFamilyName().getSurname().setValue("Mouse");
        patientName.getGivenName().setValue("Mickey");

        // Patient identifier
        pid.getPatientIdentifierList(0).getID().setValue("378785433211");

        // Patient address (XAD - Extended Address)
        XAD patientAddress = pid.getPatientAddress(0);
        patientAddress.getStreetAddress().getStreetOrMailingAddress().setValue("123 Main Street");
        patientAddress.getCity().setValue("Lake Buena Vista");
        patientAddress.getStateOrProvince().setValue("FL");
        patientAddress.getCountry().setValue("USA");
    }

    /**
     * Constructs the PV1 (Patient Visit) segment.
     * Contains visit/encounter information.
     */
    private void createPv1Segment() throws DataTypeException {
        PV1 pv1 = adtMessage.getPV1();

        // Patient class: I=Inpatient, O=Outpatient, E=Emergency
        pv1.getPatientClass().setValue("O");

        // Assigned patient location (PL - Person Location)
        PL location = pv1.getAssignedPatientLocation();
        location.getFacility().getNamespaceID().setValue("Some Treatment Facility Name");
        location.getPointOfCare().setValue("ER");

        // Admission type: E=Emergency, U=Urgent, R=Routine
        pv1.getAdmissionType().setValue("E");

        // Referring doctor (XCN - Extended Composite ID and Name)
        XCN referringDoctor = pv1.getReferringDoctor(0);
        referringDoctor.getIDNumber().setValue("99999999");
        referringDoctor.getFamilyName().getSurname().setValue("Smith");
        referringDoctor.getGivenName().setValue("Jack");
        referringDoctor.getIdentifierTypeCode().setValue("NPI");

        // Admit date/time
        pv1.getAdmitDateTime().getTimeOfAnEvent().setValue(getCurrentTimestamp());
    }

    /**
     * Gets the current timestamp in HL7 format (yyyyMMddHHmmss).
     */
    private String getCurrentTimestamp() {
        return LocalDateTime.now().format(HL7_TIMESTAMP_FORMAT);
    }

    /**
     * Generates a unique message control ID.
     * In production, use a database sequence or distributed ID generator.
     */
    private String generateMessageControlId() {
        String facilityPrefix = "FAC001";
        int randomSuffix = ThreadLocalRandom.current().nextInt(1000, 9999);
        return facilityPrefix + getCurrentTimestamp() + randomSuffix;
    }
}
