package com.saravanansubramanian.hapihl7tutorial.handlingbinarydata;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.datatype.ED;
import ca.uhn.hl7v2.model.v24.group.ORU_R01_OBSERVATION;
import ca.uhn.hl7v2.model.v24.group.ORU_R01_ORDER_OBSERVATION;
import ca.uhn.hl7v2.model.v24.message.ORU_R01;
import ca.uhn.hl7v2.model.v24.segment.OBX;
import ca.uhn.hl7v2.parser.PipeParser;

public class HapiExtractBinaryData {

	// this input file has been included in the source code for convenience
	private static String OruR01MessageWithBase64EncodedPdfReportIncluded = "C:\\HL7TestInputFiles\\SaravananOruR01Message.hl7";

	// paths for where the extracted PDF report will be written. Change these as needed
	private static String _extractedPdfOutputDirectory = "C:\\HL7TestOutputs";
	private static String _extractedPdfOutputPath = "C:\\HL7TestOutputs\\ExtractedPdfReport.pdf";

	private static HapiContext context = new DefaultHapiContext();

	public static void main(String[] args) {
		try {
			// instantiate a PipeParser, which handles the "traditional or default encoding"
			PipeParser ourPipeParser = context.getPipeParser();
			// read the message data from file and parse the string format message into a Java message object
			Message hl7Message = ourPipeParser
					.parse(readFileDataAsString(OruR01MessageWithBase64EncodedPdfReportIncluded));

			// cast to message to an ORU R01 message in order to get access to the message data
			ORU_R01 oruR01Message = (ORU_R01) hl7Message;
			
			if (oruR01Message != null) {
				// Display the updated HL7 message using Pipe delimited format
				System.out.println("Parsed HL7 Message:");
				System.out.println(ourPipeParser.encode(hl7Message));
				
				ExtractPdfDataAndWriteToFile(ourPipeParser, oruR01Message);
				return;
			}
			
			throw new IllegalStateException("Unable to access message data. Please check if your input file contains a valid ORU R01 message");
			
		} catch (Exception e) {
			System.out.println(String.format("Error occured during Order Response PDF extraction operation -> %s",
					e.getMessage()));
		}
	}

	private static void ExtractPdfDataAndWriteToFile(PipeParser ourPipeParser, ORU_R01 oruR01Message) throws HL7Exception, IOException, BadBase64EncodingException {
		
		ED encapsulatedPdfDataInBase64Format = getEncapsulatedDataFromObservationSegment(oruR01Message);

		byte[] extractedPdfByteData = getBase64DecodedPdfByteData(encapsulatedPdfDataInBase64Format);

		File directory = new File(_extractedPdfOutputDirectory);
		if (!directory.exists()) {
			System.out.println(String.format("Creating output directory at '%s'..", _extractedPdfOutputDirectory));
			new File(_extractedPdfOutputDirectory).mkdirs();
		}

		System.out.println(String.format(
				"Writing the extracted PDF data to '%s'. You should be able to see the decoded PDF content..",
				_extractedPdfOutputPath));

		writeByteDataToFile(_extractedPdfOutputPath, extractedPdfByteData);

		System.out.println("Extraction operation was successfully completed..");
	}

	private static byte[] getBase64DecodedPdfByteData(ED encapsulatedPdfDataInBase64Format) throws BadBase64EncodingException {
		OurBase64Helper helper = new OurBase64Helper();
		System.out.println("Extracting PDF data stored in Base-64 encoded form from OBX-5..");
		String base64EncodedData = encapsulatedPdfDataInBase64Format.getData().getValue();
		byte[] extractedPdfByteData = helper.ConvertFromBase64String(base64EncodedData);
		return extractedPdfByteData;
	}

	private static ED getEncapsulatedDataFromObservationSegment(ORU_R01 oruR01Message) {
		// start retrieving the OBX segment data to get at the PDF report content
		System.out.println("Extracting message data from parsed message..");
		ORU_R01_ORDER_OBSERVATION ourOrderObservation = oruR01Message.getPATIENT_RESULT().getORDER_OBSERVATION();
		ORU_R01_OBSERVATION observation = ourOrderObservation.getOBSERVATION(0);
		OBX obxSegment = observation.getOBX();
		ED encapsulatedPdfDataInBase64Format = (ED) obxSegment.getObservationValue(0).getData();
		return encapsulatedPdfDataInBase64Format;
	}

	// Note: There are better/easier methods for writing to file in the newer
	// versions of Java. I am using this simply to be backwards compatible with JDK 1.4
	private static void writeByteDataToFile(String destinationFile, byte[] byteData) throws IOException {
		FileOutputStream fileOuputStream = null;

		try {
			fileOuputStream = new FileOutputStream(destinationFile);
			fileOuputStream.write(byteData);
		} catch (IOException e) {
			// simply re-throw the exception for now
			throw e;
		} finally {
			if (fileOuputStream != null) {
				fileOuputStream.close();
			}
		}
	}
	
	public static String readFileDataAsString(String fileName) throws Exception {
		return new String(Files.readAllBytes(Paths.get(fileName)));
	}

}
