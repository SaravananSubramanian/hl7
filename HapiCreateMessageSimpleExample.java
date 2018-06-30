package com.saravanansubramanian.hapihl7tutorial;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.text.SimpleDateFormat;
import java.util.Date;
import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
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
import ca.uhn.hl7v2.parser.Parser;


public class HapiCreateMessageSimpleExample {

	private static HapiContext context;
	
	public static void main(String[] args) throws Exception {

		try {
			
			//create the ADT A01 HL7 message
			context = new DefaultHapiContext();
			
			//create ADT A01 message  
			System.out.println("Creating ADT A01 message...");
			ADT_A01 adtMessage = createAdtMessage();
			
			//create these parsers for file operations
			Parser pipeParser = context.getPipeParser();
			Parser xmlParser = context.getXMLParser();
			
			//print out the message that we constructed
			System.out.println("Message was constructed successfully..." + "\n");
			System.out.println(pipeParser.encode(adtMessage));
			
			//serialize the message to pipe delimited output file
			writeMessageToPipeDelimitedFile(pipeParser, adtMessage, "testPipeDelimitedOutputFile.txt");
			
			//serialize the message to XML format output file
			writeMessageToXmlFile(xmlParser, adtMessage,"testXmlOutputFile.xml");
			
		} catch (Exception e) {
			e.printStackTrace(); 
		}
	}

	private static ADT_A01 createAdtMessage() throws HL7Exception, IOException, DataTypeException {
		String currentDateTimeString = getCurrentTimeStamp();
		
		ADT_A01 adtMessage = new ADT_A01();

		adtMessage.initQuickstart("ADT", "A01", "P");

		// Create and populate the MSH segment
		MSH mshSegment = adtMessage.getMSH();
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
		EVN evn = adtMessage.getEVN();
		evn.getEventTypeCode().setValue("A01");
		evn.getRecordedDateTime().getTimeOfAnEvent().setValue(currentDateTimeString);

		// Create and populate the PID segment
		PID pid = adtMessage.getPID();
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
		PV1 pv1 = adtMessage.getPV1();
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

		return adtMessage;
	}
	
	private static void writeMessageToPipeDelimitedFile(Parser pipeParser, ADT_A01 adtMessage, String outputFilename)
			throws IOException, FileNotFoundException, HL7Exception {
		OutputStream outputStream = null; 
		try {
			
			//Remember that the file may not show special delimiter characters when using plain text editor
			File file = new File(outputFilename); 
			
			// quick check to create the file before writing if it does not exist already
			if (!file.exists()) {
				file.createNewFile();
			}
			
			System.out.println("Serializing message to pipe delimited file...");
			outputStream = new FileOutputStream(file);
			outputStream.write(pipeParser.encode(adtMessage).getBytes());
			outputStream.flush();
			
			System.out.printf("Message serialized to pipe delimited file '%s' successfully",file);
			System.out.println("\n");
		}	
		finally {
			if (outputStream != null) {
				outputStream.close();
			}
		}
	}
	
	private static void writeMessageToXmlFile(Parser xmlParser, ADT_A01 adtMessage, String outputFilename)
			throws IOException, FileNotFoundException, HL7Exception {
		OutputStream outputStream = null; 
		try {
			
			File file = new File(outputFilename); 
			
			// quick check to create the file before writing if it does not exist already
			if (!file.exists()) {
				file.createNewFile();
			}
			
			System.out.println("Serializing message to XML file...");
			outputStream = new FileOutputStream(file);
			outputStream.write(xmlParser.encode(adtMessage).getBytes());
			outputStream.flush();
			
			System.out.printf("Message serialized to xml format file '%s' successfully",file);
		}	
		finally {
			if (outputStream != null) {
				outputStream.close();
			}
		}
	}

	public static String getCurrentTimeStamp() {
		return new SimpleDateFormat("yyyyMMddHHmmss").format(new Date());
	}

	public static String getSequenceNumber() {
		String facilityNumberPrefix = "1234"; // some arbitrary prefix for the facility
		return facilityNumberPrefix.concat(getCurrentTimeStamp());
	}

}
