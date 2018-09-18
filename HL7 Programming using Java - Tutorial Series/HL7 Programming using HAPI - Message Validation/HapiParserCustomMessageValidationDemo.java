package com.saravanansubramanian.hapihl7tutorial.validation;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.Version;
import ca.uhn.hl7v2.parser.PipeParser;
import ca.uhn.hl7v2.validation.builder.support.DefaultValidationBuilder;

public class HapiParserCustomMessageValidationDemo {

	private static HapiContext context = new DefaultHapiContext();
	
	public static void main(String[] args) {
		
		String adtMessage
		= "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|"
			+ "RECEIVING_APPLICATION|RECEIVING_FACILITY|20110613083617|"
			+ "|ADT^A04|934576120110613083617|P|2.3||||\r\n" + 
			"EVN|A04|20110613083617|||\r\n" + 
			"PID|1||135769||MOUSE^MICKEY^||19281118|M|||"
			+ "123 Main St.^^Lake Buena Vista^FL^32830||"
			+ "(407)939-1289|||||1719|99999999||||||||||||||||||||\r\n" + 
			"PV1|1|O|||||7^Disney^Walt^^MD^^^^|";

		//specify an override for our default validation behavior by injecting our own extension
		context.setValidationRuleBuilder(new OurSpecialMessageValidationBuilderClass());

		try {
			PipeParser parser = context.getPipeParser();
			parser.parse(adtMessage);
			System.out.println("Code should not get here");
		} catch (HL7Exception e) {
			System.out.println("Validation failed as expected during parsing since PV1-3 is now mandatory");
			System.out.println("Validation Message: " + e.getMessage());
		}
	}
	

}

@SuppressWarnings("serial")
class OurSpecialMessageValidationBuilderClass extends DefaultValidationBuilder{

	@Override
	protected void configure() {
		super.configure();
		forVersion(Version.V23)
		    .message("ADT", "A04")
		    .terser("PV1-3", not(empty()));
	}
}
