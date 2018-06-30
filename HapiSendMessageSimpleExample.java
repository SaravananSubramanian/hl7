package com.saravanansubramanian.hapihl7tutorial;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.model.v24.datatype.PL;
import ca.uhn.hl7v2.model.v24.datatype.XAD;
import ca.uhn.hl7v2.model.v24.datatype.XCN;
import ca.uhn.hl7v2.model.v24.datatype.XPN;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.model.v24.segment.EVN;
import ca.uhn.hl7v2.model.v24.segment.MSH;
import ca.uhn.hl7v2.model.v24.segment.PID;
import ca.uhn.hl7v2.model.v24.segment.PV1;
import ca.uhn.hl7v2.parser.Parser;
import ca.uhn.hl7v2.app.*;
import ca.uhn.hl7v2.model.DataTypeException;
import ca.uhn.hl7v2.model.Message;


public class HapiSendMessageSimpleExample {

	private static final int PORT_NUMBER = 52463;//change this to whatever your port number is
	private static HapiContext context;
	
	public static void main(String[] args) throws Exception {

		//create a HL7 message
		context = new DefaultHapiContext();
		Parser parser = context.getPipeParser();
		ADT_A01 adtMessage = createAdtMessage(parser);

		//create a new MLLP client over the specified port
		Connection connection = context.newClient("localhost", PORT_NUMBER, false);

		// The initiator is used to transmit unsolicited messages
		Initiator initiator = connection.getInitiator();
		
		//send the previously created HL7 message over the connection established
		System.out.println("Sending message:" + "\n" + parser.encode(adtMessage));
		Message response = initiator.sendAndReceive(adtMessage);
		
		//display the message response received from the remote party
		String responseString = parser.encode(response);
		System.out.println("Received response:\n" + responseString);

	}

	private static ADT_A01 createAdtMessage(Parser parser) throws HL7Exception, IOException, DataTypeException {
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
		XPN patientName = pid.getPatientName(0);
		patientName.getFamilyName().getSurname().setValue("Mouse");
		patientName.getGivenName().setValue("Mickey");
		pid.getPatientIdentifierList(0).getID().setValue("378785433211");
		XAD patientAddress = pid.getPatientAddress(0);
		patientAddress.getStreetAddress().getStreetOrMailingAddress().setValue("123 Main Street");
		patientAddress.getCity().setValue("Lake Buena Vista");
		patientAddress.getStateOrProvince().setValue("FL");
		patientAddress.getCountry().setValue("USA");

		// Create and populate the PV1 segment
		PV1 pv1 = adt.getPV1();
		pv1.getPatientClass().setValue("O"); // to represent an 'Outpatient'
		PL assignedPatientLocation = pv1.getAssignedPatientLocation();
		assignedPatientLocation.getFacility().getNamespaceID().setValue("Some Treatment Facility Name");
		assignedPatientLocation.getPointOfCare().setValue("Some Point of Care");
		pv1.getAdmissionType().setValue("ALERT");
		XCN referringDoctor = pv1.getReferringDoctor(0);
		referringDoctor.getIDNumber().setValue("99999999");
		referringDoctor.getFamilyName().getSurname().setValue("Smith");
		referringDoctor.getGivenName().setValue("Jack");
		referringDoctor.getIdentifierTypeCode().setValue("456789");
		pv1.getAdmitDateTime().getTimeOfAnEvent().setValue(getCurrentTimeStamp());

		return adt;
	}

	public static String getCurrentTimeStamp() {
		return new SimpleDateFormat("yyyyMMddHHmmss").format(new Date());
	}

	public static String getSequenceNumber() {
		String facilityNumberPrefix = "1234"; // some arbitrary prefix for the facility
		return facilityNumberPrefix.concat(getCurrentTimeStamp());
	}

}
