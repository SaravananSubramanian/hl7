package com.saravanansubramanian.hapihl7tutorial.listeners;

import com.saravanansubramanian.hapihl7tutorial.create.AdtMessageFactory;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.app.HL7Service;
import ca.uhn.hl7v2.app.Initiator;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;

public class BasicListenerWithoutMessageHandling {

	// change this to whatever your port number is
	private static final int PORT_NUMBER = 61386;

	// In HAPI, almost all things revolve around a context object
	private static HapiContext context = new DefaultHapiContext();

	public static void main(String[] args) throws Exception {

		try {
			
			boolean useSecureConnection = false; // are you using TLS/SSL?

			Connection ourConnection = context.newLazyClient("localhost", PORT_NUMBER, useSecureConnection);
			Initiator initiator = ourConnection.getInitiator();

			HL7Service ourHl7Server = context.newServer(PORT_NUMBER, useSecureConnection);

			ourHl7Server.startAndWait();

			ADT_A01 adtMessage = (ADT_A01) AdtMessageFactory.createMessage("A01");
			
			Parser ourPipeParser = context.getPipeParser();
			Message messageResponse = initiator.sendAndReceive(adtMessage);

			String responseString = ourPipeParser.encode(messageResponse);
			System.out.println("Received a message response:\n" + responseString);

			ourConnection.close();
			
			ourHl7Server.stopAndWait();
			
		} catch (Exception e) {
			
			e.printStackTrace();
		}

	}

}
