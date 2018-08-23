package com.saravanansubramanian.hapihl7tutorial.listeners.helpers;

import java.util.Map;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.protocol.ReceivingApplicationExceptionHandler;

public class OurExceptionHandler implements ReceivingApplicationExceptionHandler {

	@Override
	public String processException(String theIncomingMessage, Map<String, Object> theIncomingMetadata, String theOutgoingNegativeAcknowledgementMessage, Exception theException)
            throws HL7Exception {
        
        System.out.println("The error message was:" + theException.getMessage() + "\n");
        
        //do any additional error processing such as error logging, alerting, etc here
        //based on the exception type or the exception message
        //use or extend the HL7Exception class in your message processing logic where possible
        //so that logic here is short and simple
        
        //you can return the outgoing message as is, or throw a customized error message
        //you have to return something from this method no matter what
        
        return theOutgoingNegativeAcknowledgementMessage;
    }

}
