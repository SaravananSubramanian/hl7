package com.saravanansubramanian.hapihl7tutorial.conformanceprofile;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.conf.check.DefaultValidator;
import ca.uhn.hl7v2.conf.parser.ProfileParser;
import ca.uhn.hl7v2.conf.spec.RuntimeProfile;
import ca.uhn.hl7v2.model.v23.message.ADT_A01;
import ca.uhn.hl7v2.parser.PipeParser;
import java.nio.file.*;

public class HapiConformanceProfileBasicExampleOfValidationFailure {

	public static void main(String[] args) throws Exception {

		try {

			// Load the conformance profile
			ProfileParser ourProfileParser = new ProfileParser(false);
			RuntimeProfile ourConformanceProfile = ourProfileParser.
					parseClasspath("com/saravanansubramanian/hapihl7tutorial/"
							+ "Saravanan Adv Testing Example profile - ADT_A01.xml");

			//Read a test non-conformant HL7 from file. See my GitHub page for this file
			String message = readHl7FileDataAsString("C:\\HL7TestInputFiles\\FileWithNonConformingAdtA01Message.txt"); 
			
			//parse the HL7 message from the file data
			ADT_A01 msg = (ADT_A01) (new PipeParser()).parse(message);

			// Validate the HL7 message using the sample HL7 conformance profile I have provided. See my GitHub page for the XML file
			HL7Exception[] errors = new DefaultValidator().validate(msg, ourConformanceProfile.getMessage());

			// Display all the validation errors that are generated.
			System.out.println("The following validation errors were found during message validation:");
			
			for (HL7Exception hl7Exception : errors) {
				System.out.println(hl7Exception);
			}
			

		} catch (Exception e) {
			e.printStackTrace();
		}

	}
	
	public static String readHl7FileDataAsString(String fileName) throws Exception
	  {
	    String data = new String(Files.readAllBytes(Paths.get(fileName)));
	    return data;
	  }

}
