package com.saravanansubramanian.hapihl7tutorial.send;

import com.saravanansubramanian.hapihl7tutorial.create.AdtMessageFactory;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.app.Initiator;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;

public class HapiSendMessageSimpleExample {

	private static final int PORT_NUMBER = 52463;// change this to whatever your port number is

	// In HAPI, almost all things revolve around a context object
	private static HapiContext context = new DefaultHapiContext();
		
	public static void main(String[] args) throws Exception {

		try {

			// create the HL7 message
			// this AdtMessageFactory class is not from HAPI but my own wrapper
			// check my GitHub page or see my earlier article for reference
			ADT_A01 adtMessage = AdtMessageFactory.createMessage("A01");

			// create a new MLLP client over the specified port
			Connection connection = context.newClient("localhost", PORT_NUMBER, false);

			// The initiator which will be used to transmit our message
			Initiator initiator = connection.getInitiator();

			// send the previously created HL7 message over the connection established
			Parser parser = context.getPipeParser();
			System.out.println("Sending message:" + "\n" + parser.encode(adtMessage));
			Message response = initiator.sendAndReceive(adtMessage);

			// display the message response received from the remote party
			String responseString = parser.encode(response);
			System.out.println("Received response:\n" + responseString);

		} catch (Exception e) {
			e.printStackTrace();
		}

	}

}
