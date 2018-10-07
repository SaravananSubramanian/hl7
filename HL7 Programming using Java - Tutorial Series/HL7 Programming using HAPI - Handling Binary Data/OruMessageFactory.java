package com.saravanansubramanian.hapihl7tutorial.handlingbinarydata;

import java.io.IOException;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.Message;

public class OruMessageFactory {
	//you will pass in parameters here in the form of a DTO or domain object
    //for message construction in your implementation
    public static Message CreateMessage() throws HL7Exception, IOException
    {
        return new OurOruR01MessageBuilder().Build();
    }
}
