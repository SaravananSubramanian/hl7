package com.saravanansubramanian.hapihl7tutorial.listeners.helpers;

import java.io.IOException;
import java.util.Map;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.protocol.ReceivingApplication;
import ca.uhn.hl7v2.protocol.ReceivingApplicationException;

public class OurSimpleApplication implements ReceivingApplication {

	private static HapiContext context = new DefaultHapiContext();
	
	@Override
	public boolean canProcess(Message message) {
		return true;
	}

	@Override
	public Message processMessage(Message receivedMessage, Map<String, Object> metaData)
			throws ReceivingApplicationException, HL7Exception {
		
		String receivedEncodedMessage = context.getPipeParser().encode(receivedMessage);
        System.out.println("Incoming message:\n" + receivedEncodedMessage + "\n\n");
        
        try {
        	return receivedMessage.generateACK();
        } catch (IOException e) {
            throw new HL7Exception(e);
        }
        
	}

}