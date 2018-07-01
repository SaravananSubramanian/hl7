package com.saravanansubramanian.hapihl7tutorial;

import java.io.IOException;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;

public class AdtMessageFactory {

	public static ADT_A01 createMessage(String messageType) throws HL7Exception, IOException {
		
		//This patterns enables you to build other message types 
		if ( messageType.equals("A01") )
		{
			return new OurAdtA01MessageBuilder().Build();
		}
		
		throw new RuntimeException(messageType + " is not supported yet. Extend this if you need to");
		
	}
}
