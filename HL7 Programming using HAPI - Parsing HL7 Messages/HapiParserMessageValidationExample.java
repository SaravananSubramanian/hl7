package com.saravanansubramanian.hapihl7tutorial.parsers;

import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.message.ACK;
import ca.uhn.hl7v2.model.v24.segment.MSH;
import ca.uhn.hl7v2.parser.DefaultXMLParser;
import ca.uhn.hl7v2.parser.PipeParser;
import ca.uhn.hl7v2.parser.XMLParser;

public class HapiParserMessageValidationExample {

	public static void main(String[] args) {
		
		String messageString = "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|20110614075841||ACK|1407511|P|2.4||||||\r\n" + 
				"MSA|AA|1407511|Success||";

		// instantiate a PipeParser, which handles the "traditional or default encoding"
		PipeParser ourPipeParser = new PipeParser();

		try {
			// parse the string format message into a Java message object
			Message hl7Message = ourPipeParser.parse(messageString);

			//cast to ACK message to get access to ACK message data
			if (hl7Message instanceof ACK) {
				ACK ackResponseMessage = (ACK) hl7Message;
				
				//access message data and display it
				//note that I am using encode method at the end to convert it back to string for display
				MSH mshSegmentMessageData = ackResponseMessage.getMSH();
				System.out.println("Message Type is " + mshSegmentMessageData.getMessageType().encode());
				System.out.println("Message Control Id is " + mshSegmentMessageData.getMessageControlID().encode());
				System.out.println("Message Timestamp is " + mshSegmentMessageData.getDateTimeOfMessage().encode());
				System.out.println("Sending Facility is " + mshSegmentMessageData.getSendingFacility().encode() + "\n");
				
				//update message data in MSA segment
				ackResponseMessage.getMSA().getAcknowledgementCode().setValue("AR");
				
			}

			// instantiate an XML parser
			//HAPI provides 
			XMLParser xmlParser = new DefaultXMLParser();

			// convert from default encoded message into XML format, and send it to standard out for display
			System.out.println(xmlParser.encode(hl7Message));
			
			System.out.println("Printing Message Structure in Abstract Message Syntax Format... ");
			System.out.println(hl7Message.printStructure());
			
		} catch (Exception e) {
			e.printStackTrace();
		}

	}

}
