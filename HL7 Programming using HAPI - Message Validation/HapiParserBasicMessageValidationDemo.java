package com.saravanansubramanian.hapihl7tutorial.validation;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.parser.PipeParser;
import ca.uhn.hl7v2.validation.ValidationContext;
import ca.uhn.hl7v2.validation.impl.ValidationContextFactory;

public class HapiParserBasicMessageValidationDemo {

	private static HapiContext context = new DefaultHapiContext();
	
	public static void main(String[] args) {
		
		//We will look at four scenarios 
		
		parsingValidMessage_Scenario1();
		
		parsingInvalidAttribute_Scenario2();
		
		parsingInvalidMessageWithValidationTurnedOff_Scenario3();
	}

	private static void parsingValidMessage_Scenario1() {
		
		String aValidAcknowledgementMessage = "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|"
				+ "RECEIVING_APPLICATION|RECEIVING_FACILITY|"
				+ "20110614075841||ACK|1407511|P|2.4||||||\r\n" + 
				"MSA|AA|1407511|Success||";

		context.setValidationContext((ValidationContext)ValidationContextFactory.defaultValidation());

		try {
			PipeParser parser = context.getPipeParser();
			parser.parse(aValidAcknowledgementMessage);
			System.out.println("Scenario 1. Successfully validated a correct HL7 acknowledgement message");
		} catch (HL7Exception e) {
			System.out.println("Scenario 1. Validation failed during parsing:" + e.getMessage());
		}
	}
	
	private static void parsingInvalidAttribute_Scenario2() {
		
		//intentionally create bad data in the MSH-6 field which is a TS (Timestamp) data type 
		String anInvalidAcknowledgementMessage = "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|"
				+ "RECEIVING_APPLICATION|RECEIVING_FACILITY|"
				+ "AAAAAAAAAAAAA||ACK|1407511|P|2.4||||||\r\n" + 
				"MSA|AA|1407511|Success||";

		try {
			PipeParser parser = context.getPipeParser();
			parser.parse(anInvalidAcknowledgementMessage);
			System.out.println("Scenario 2. The code show not get here as the validation will fail");
		} catch (HL7Exception e) {
			System.out.println("Scenario 2. Validation failed during parsing:" + e.getMessage());
		}
	}
	
	private static void parsingInvalidMessageWithValidationTurnedOff_Scenario3() {
		
		//intentionally create bad data in the MSH-6 field which is a TS (Timestamp) data type 
		String anInvalidAcknowledgementMessage = "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|"
				+ "RECEIVING_APPLICATION|RECEIVING_FACILITY|"
				+ "AAAAAAAAAAAAA||ACK|1407511|P|2.4||||||\r\n" + 
				"MSA|AA|1407511|Success||";
		
		//now turn off validation off
		context.getParserConfiguration().setValidating(false);

		try {
			PipeParser parser = context.getPipeParser();
			parser.parse(anInvalidAcknowledgementMessage);
			System.out.println("Scenario 3. By disabling validation, we successfully parsed an invalid HL7 message");
		} catch (HL7Exception e) {
			System.out.println("Scenario 3. Validation failed during parsing:" + e.getMessage());
		}
	}

}
