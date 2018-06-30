package com.saravanansubramanian.hapihl7tutorial;

import java.text.SimpleDateFormat;
import java.util.Date;
import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.model.v24.segment.EVN;
import ca.uhn.hl7v2.model.v24.segment.MSH;
import ca.uhn.hl7v2.model.v24.segment.PID;
import ca.uhn.hl7v2.model.v24.segment.PV1;
import ca.uhn.hl7v2.parser.Parser;

public class hapicreatenewmessage {

	private static HapiContext context;
	private static String facilityNumberPrefix = "1234"; // some arbitrary prefix for the facility

	public static void main(String[] args) throws Exception {

		String currentDateTimeString = getCurrentTimeStamp();

		ADT_A01 adt = new ADT_A01();

		adt.initQuickstart("ADT", "A01", "P");

		// Create and populate the MSH segment
		MSH mshSegment = adt.getMSH();
		mshSegment.getFieldSeparator().setValue("|");
		mshSegment.getEncodingCharacters().setValue("^~\\&");
		mshSegment.getSendingApplication().getNamespaceID().setValue("Our System");
		mshSegment.getSendingFacility().getNamespaceID().setValue("Our Facility");
		mshSegment.getReceivingApplication().getNamespaceID().setValue("Their Remote System");
		mshSegment.getReceivingFacility().getNamespaceID().setValue("Their Remote Facility");
		mshSegment.getDateTimeOfMessage().getTimeOfAnEvent().setValue(currentDateTimeString);
		mshSegment.getMessageControlID().setValue(getSequenceNumber());
		mshSegment.getVersionID().getVersionID().setValue("2.4");

		// Create and populate the EVN segment
		EVN evn = adt.getEVN();
		evn.getEventTypeCode().setValue("A01");
		evn.getRecordedDateTime().getTimeOfAnEvent().setValue(currentDateTimeString);

		// Create and populate the PID segment
		PID pid = adt.getPID();
		pid.getPatientName(0).getFamilyName().getSurname().setValue("Mouse");
		pid.getPatientName(0).getGivenName().setValue("Mickey");
		pid.getPatientIdentifierList(0).getID().setValue("378785433211");

		// Create and populate the PV1 segment
		PV1 pv1 = adt.getPV1();
		pv1.getPatientClass().setValue("O"); // to represent an 'Outpatient'
		pv1.getAssignedPatientLocation().getFacility().getNamespaceID().setValue("Some Treatment Facility Name");
		pv1.getAssignedPatientLocation().getPointOfCare().setValue("Some Point of Care");
		pv1.getAdmissionType().setValue("ALERT");
		pv1.getReferringDoctor(0).getIDNumber().setValue("99999999");
		pv1.getReferringDoctor(0).getFamilyName().getSurname().setValue("Smith");
		pv1.getReferringDoctor(0).getGivenName().setValue("Jack");
		pv1.getReferringDoctor(0).getIdentifierTypeCode().setValue("456789");
		pv1.getAdmitDateTime().getTimeOfAnEvent().setValue(getCurrentTimeStamp());

		// use the HAPI context class to encode and print the message to the console
		context = new DefaultHapiContext();
		Parser parser = context.getPipeParser();
		String encodedMessage = parser.encode(adt);

		// print out the message that we constructed
		System.out.println(encodedMessage);
		
	}

	public static String getCurrentTimeStamp() {
		return new SimpleDateFormat("yyyyMMddHHmmss").format(new Date());
	}

	public static String getSequenceNumber() {
		return facilityNumberPrefix.concat(getCurrentTimeStamp());
	}

}
