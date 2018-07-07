package com.saravanansubramanian.hapihl7tutorial.listeners;

import com.saravanansubramanian.hapihl7tutorial.create.AdtMessageFactory;
import com.saravanansubramanian.hapihl7tutorial.listeners.helpers.OurSimpleApplication;
import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.app.Connection;
import ca.uhn.hl7v2.app.HL7Service;
import ca.uhn.hl7v2.app.Initiator;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;
import ca.uhn.hl7v2.protocol.ApplicationRouter.AppRoutingData;

public class BasicListenerWithEnhancedMessageHandlingAndRouting {

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

			//You can set up routing rules for your HL7 listener by extending the AppRoutingData class like this
            ourHl7Server.registerApplication(new RegistrationEventRoutingData(), new OurSimpleApplication());
            
            //You can also set up the same routing logic like below
            //Try several applications all processing different message versions, types and trigger events on your own
            //AppRoutingDataImpl ourRouter = new AppRoutingDataImpl("ADT", "A0.", "P", "2.4");
            //ourHl7Server.registerApplication(ourRouter, new OurSimpleApplication());
			
			ourHl7Server.startAndWait();

			ADT_A01 adtMessage = AdtMessageFactory.createMessage("A01");
			
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

class RegistrationEventRoutingData implements AppRoutingData {
    
    //Note, all conditions must cumulatively be true 
    //for a message to be processed
    
    @Override
    public String getVersion() {
        //process HL7 2.4 version messages only
        return "2.4";
    }
    
    @Override
    public String getTriggerEvent() {
        // you can use regular matching for your routing 
        //only trigger events that start with 'A0' will be processed
        return "A0."; 
    }
    
    @Override
    public String getProcessingId() {
        //process all messages regardless of processing id
        return "*"; 
    }
    
    @Override
    public String getMessageType() {
        //process only ADT message types
        return "ADT";
    }
}


