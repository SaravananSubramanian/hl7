package com.saravanansubramanian.hapihl7tutorial.create;

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
		
		//if other types of ADT messages are needed, then implement your builders here
		throw new RuntimeException(String.format("%s message type is not supported yet. Extend this if you need to", messageType));
		
	}
}
