package com.saravanansubramanian.hapihl7tutorial.handlingbinarydata;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.app.Initiator;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.parser.Parser;

public class HapiSendBinaryData {

	private static int PORT_NUMBER = 57550;// change this to whatever your port number is

	// In HAPI, almost all things revolve around a context object
	private static HapiContext context = new DefaultHapiContext();

	public static void main(String[] args) {
		try {
			// create the HL7 message
			// this OruMessageFactory class is not from NHAPI but my own wrapper class
			// check my GitHub page or see my earlier article for reference
			Message oruMessage = OruMessageFactory.CreateMessage();

			// create a new MLLP client over the specified port
			Connection connection = context.newClient("localhost", PORT_NUMBER, false);

			// The initiator which will be used to transmit our message
			Initiator initiator = connection.getInitiator();

			// send the previously created HL7 message over the connection established
			Parser parser = context.getPipeParser();
			System.out.println("Sending message:" + "\n" + parser.encode(oruMessage));
			Message response = initiator.sendAndReceive(oruMessage);

			// display the message response received from the remote party
			String responseString = parser.encode(response);
			System.out.println("Received response:\n" + responseString);

		} catch (Exception e) {
			e.printStackTrace();
		}

	}

}
