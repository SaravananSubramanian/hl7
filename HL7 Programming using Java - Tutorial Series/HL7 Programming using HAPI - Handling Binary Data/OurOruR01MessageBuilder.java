package com.saravanansubramanian.hapihl7tutorial.handlingbinarydata;

import java.io.File;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.DataTypeException;
import ca.uhn.hl7v2.model.Varies;
import ca.uhn.hl7v2.model.v24.datatype.ED;
import ca.uhn.hl7v2.model.v24.datatype.PL;
import ca.uhn.hl7v2.model.v24.datatype.XAD;
import ca.uhn.hl7v2.model.v24.datatype.XCN;
import ca.uhn.hl7v2.model.v24.datatype.XPN;
import ca.uhn.hl7v2.model.v24.group.ORU_R01_OBSERVATION;
import ca.uhn.hl7v2.model.v24.group.ORU_R01_ORDER_OBSERVATION;
import ca.uhn.hl7v2.model.v24.message.ORU_R01;
import ca.uhn.hl7v2.model.v24.segment.MSH;
import ca.uhn.hl7v2.model.v24.segment.OBR;
import ca.uhn.hl7v2.model.v24.segment.OBX;
import ca.uhn.hl7v2.model.v24.segment.PID;
import ca.uhn.hl7v2.model.v24.segment.PV1;

public class OurOruR01MessageBuilder {
	private ORU_R01 _oruR01Message;
	private OurBase64Helper _ourBase64Helper = new OurBase64Helper();

	/*
	 * You can pass in a domain or data transfer object as a parameter when
	 * integrating with data from your application here I will leave that to you to
	 * explore on your own Using fictional data here for illustration
	 */

	public ORU_R01 Build() throws HL7Exception, IOException {
		String currentDateTimeString = getCurrentTimeStamp();
		_oruR01Message = new ORU_R01();
		// you can use the context class's newMessage method to instantiate a message if
		// you want
		_oruR01Message.initQuickstart("ORU", "R01", "P");

		CreateMshSegment(currentDateTimeString);
		CreatePidSegment();
		CreatePv1Segment();
		CreateObrSegment();
		CreateObxSegment();
		return _oruR01Message;
	}

	private void CreateMshSegment(String currentDateTimeString) throws DataTypeException {
		MSH mshSegment = _oruR01Message.getMSH();
		mshSegment.getFieldSeparator().setValue("|");
		mshSegment.getEncodingCharacters().setValue("^~\\&");
		mshSegment.getSendingApplication().getNamespaceID().setValue("Our System");
		mshSegment.getSendingFacility().getNamespaceID().setValue("Our Facility");
		mshSegment.getReceivingApplication().getNamespaceID().setValue("Their Remote System");
		mshSegment.getReceivingFacility().getNamespaceID().setValue("Their Remote Facility");
		mshSegment.getDateTimeOfMessage().getTimeOfAnEvent().setValue(currentDateTimeString);
		mshSegment.getMessageControlID().setValue(getSequenceNumber());
		mshSegment.getVersionID().getVersionID().setValue("2.4");
	}

	private void CreatePidSegment() throws DataTypeException {
		PID pid = _oruR01Message.getPATIENT_RESULT().getPATIENT().getPID();
		XPN patientName = pid.getPatientName(0);
		patientName.getFamilyName().getSurname().setValue("Mouse");
		patientName.getGivenName().setValue("Mickey");
		pid.getPatientIdentifierList(0).getID().setValue("378785433211");
		XAD patientAddress = pid.getPatientAddress(0);
		patientAddress.getStreetAddress().getStreetOrMailingAddress().setValue("123 Main Street");
		patientAddress.getCity().setValue("Lake Buena Vista");
		patientAddress.getStateOrProvince().setValue("FL");
		patientAddress.getCountry().setValue("USA");
	}

	private void CreatePv1Segment() throws DataTypeException {
		PV1 pv1 = _oruR01Message.getPATIENT_RESULT().getPATIENT().getVISIT().getPV1();
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
	}

	private void CreateObrSegment() throws DataTypeException {
		ORU_R01_ORDER_OBSERVATION orderObservation = _oruR01Message.getPATIENT_RESULT().getORDER_OBSERVATION();
		OBR obr = orderObservation.getOBR();
		obr.getSetIDOBR().setValue("1");
		obr.getPlacerOrderNumber().getUniversalID().setValue("9434934");
		obr.getFillerOrderNumber().getUniversalID().setValue("123456");
		obr.getUniversalServiceIdentifier().getText().setValue("Document");
		obr.getObservationEndDateTime().getTimeOfAnEvent().setValue(getCurrentTimeStamp());
		;
		obr.getResultStatus().setValue("F");
	}

	private void CreateObxSegment() throws DataTypeException, IOException {
		ORU_R01_OBSERVATION observation = _oruR01Message.getPATIENT_RESULT().getORDER_OBSERVATION().getOBSERVATION(0);
		OBX obx = observation.getOBX();
        obx.getSetIDOBX().setValue("1");
        obx.getValueType().setValue("ED");
        obx.getObservationIdentifier().getIdentifier().setValue("Report");
        Varies value = obx.getObservationValue(0);
        ED encapsulatedData = new ED(_oruR01Message);
        String base64EncodedStringOfPdfReport = _ourBase64Helper.ConvertToBase64String(new File("C:\\HL7TestInputFiles\\Sample Pathology Lab Report.pdf"));
		encapsulatedData.getData().setValue(base64EncodedStringOfPdfReport);
        value.setData(encapsulatedData);
	}

	private String getCurrentTimeStamp() {
		return new SimpleDateFormat("yyyyMMddHHmmss").format(new Date());
	}

	private String getSequenceNumber() {
		String facilityNumberPrefix = "1234"; // some arbitrary prefix for the facility
		return facilityNumberPrefix.concat(getCurrentTimeStamp());
	}
}
