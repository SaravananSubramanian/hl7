package com.saravanansubramanian.hapihl7tutorial.listeners;

import java.util.Map;
import com.saravanansubramanian.hapihl7tutorial.create.AdtMessageFactory;
import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.app.ConnectionListener;
import ca.uhn.hl7v2.app.HL7Service;
import ca.uhn.hl7v2.app.Initiator;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;
import ca.uhn.hl7v2.protocol.ReceivingApplication;
import ca.uhn.hl7v2.protocol.ReceivingApplicationException;
import ca.uhn.hl7v2.protocol.impl.AppRoutingDataImpl;

public class BasicListenerShowingDefaultErrorHandling {

	// change this to whatever your port number is
	private static final int PORT_NUMBER = 56420;

	// In HAPI, almost all things revolve around a context object
	private static HapiContext context = new DefaultHapiContext();

	public static void main(String[] args) throws Exception {

		try {
			boolean useSecureConnection = false; // are you using TLS/SSL?

			Connection ourConnection = context.newLazyClient("localhost", PORT_NUMBER, useSecureConnection);
			Initiator initiator = ourConnection.getInitiator();

			HL7Service ourHl7Server = context.newServer(PORT_NUMBER, useSecureConnection);

			AppRoutingDataImpl ourRouter = new AppRoutingDataImpl("ADT", "A0.", "P", "2.4");
            
			//call a dummy application that simulates an exception scenario
			ourHl7Server.registerApplication(ourRouter, new AnErrorThrowingApplication());
			
			//register a connection listener to the listener as well
			//this lets us listen for connection-related events 
			//such as when a new connection is being opened 
			//or when a connection is being closed and discarded
            ourHl7Server.registerConnectionListener(new OurConnectionListener());
			
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

class AnErrorThrowingApplication implements ReceivingApplication {

	@Override
	public boolean canProcess(Message arg0) {
		return true;
	}

	@Override
	public Message processMessage(Message incomingMessage, Map<String, Object> messageMetaData)
			throws ReceivingApplicationException, HL7Exception {
		System.out.println("Received incoming message:\n" + incomingMessage);
		
		//intentionally raise an exception here to see what the default message acknowledgement looks like
		throw new RuntimeException("Some Error Thrown Here. This will be returned in the ERR segment of the message response");
	}

}

class OurConnectionListener implements ConnectionListener {

	@Override
	public void connectionDiscarded(Connection connectionBeingDiscarded) {
		System.out.println("Connection discarded event fired " + connectionBeingDiscarded.getRemoteAddress());
		System.out.println("For Remote Address: " + connectionBeingDiscarded.getRemoteAddress());
		System.out.println("For Remote Port: " + connectionBeingDiscarded.getRemotePort());
	}

	@Override
	public void connectionReceived(Connection connectionBeingOpened) {
		System.out.println("Connection opened event fired " + connectionBeingOpened.getRemoteAddress());
		System.out.println("From Remote Address: " + connectionBeingOpened.getRemoteAddress());
		System.out.println("From Remote Port: " + connectionBeingOpened.getRemotePort());
	}

}


