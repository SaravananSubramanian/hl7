package com.saravanansubramanian.hapihl7tutorial.parsers;

import com.saravanansubramanian.hapihl7tutorial.parsers.custommodel.v25.message.ZDT_A01;
import com.saravanansubramanian.hapihl7tutorial.parsers.custommodel.v25.segment.ZPV;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.parser.CustomModelClassFactory;
import ca.uhn.hl7v2.parser.ModelClassFactory;
import ca.uhn.hl7v2.parser.Parser;

public class HapiParserCustomMessageModelExample {

	private static HapiContext context = new DefaultHapiContext();
	
    public static void main(String[] args) throws HL7Exception {

        Parser parser = context.getPipeParser();

        ModelClassFactory ourCustomModelClassFactory = new CustomModelClassFactory("com.saravanansubramanian.hapihl7tutorial.parsers.custommodel");
        context.setModelClassFactory(ourCustomModelClassFactory);

        String messageText = "MSH|^~\\&|IRIS|SANTER|AMB_R|SANTER|200803051508||ZDT^A01|263206|P|2.5\r"
				+ "EVN||200803051509||||200803031508\r"
				+ "PID|||5520255^^^PK^PK~ZZZZZZ83M64Z148R^^^CF^CF~ZZZZZZ83M64Z148R^^^SSN^SSN^^20070103^99991231~^^^^TEAM||ZZZ^ZZZ||19830824|F||||||||||||||||||||||N\r"
                + "ZPV|Some Custom Notes|Additional custom description of the visit goes here";
		
        //parse this message information into our ZDT custom message
        System.out.println("Attempting to parse message string into HL7 message object...");
        ZDT_A01 zdtA01Message = (ZDT_A01) parser.parse(messageText);
        System.out.println("ZDT^A01 message was parsed successfully" + "\n");
        
        //extract the ZPV Z-segment from this parsed message
        System.out.println("Retrieving the Z-segment from this message...");
        ZPV zpvSegment = zdtA01Message.getZPVSegment();
        System.out.println("Z-segment ZPV was retrieved successfully..." + "\n");

        //print the extracted custom fields from the Z-segment below
        System.out.println("Custom Notes retrieved from ZPV segment was -> " + zpvSegment.getCustomNotes()[0].encode()); // Print custom notes
        System.out.println("Custom Description retrieved from ZPV segment was -> " + zpvSegment.getCustomDescription()[0].encode()); // Print custom phone number

    }

}

