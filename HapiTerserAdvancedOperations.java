package com.saravanansubramanian.hapihl7tutorial.tersers;

import java.nio.file.Files;
import java.nio.file.Paths;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.parser.PipeParser;
import ca.uhn.hl7v2.util.Terser;

public class HapiTerserAdvancedOperations {

	public static void main(String[] args) {
		
		try {
			
			//see my GitHub page for this file
			String messageString = readHL7MessageFromFileAsString("C:\\HL7TestInputFiles\\FileWithObservationResultMessage.txt");
			
			// instantiate a PipeParser, which handles the "traditional or default encoding"
			PipeParser ourPipeParser = new PipeParser();
						
			// parse the message string into a Java message object
			Message orderResultsHl7Message = ourPipeParser.parse(messageString);
			
			//create a terser object instance by wrapping it around the message object
			Terser terser = new Terser(orderResultsHl7Message);
			
			//now, let us do various operations on the message
			OurTerserHelper terserDemonstrator = new OurTerserHelper(terser);
			
			String terserExpression = "/.OBSERVATION(1)/OBX-3";
			String dataRetrieved = terserDemonstrator.getData(terserExpression);
			System.out.printf("Observation group's 2nd OBX segment's second field using expression '%s' was: '%s' \n\n",terserExpression, dataRetrieved);
			
			terserExpression = "/.OBSERVATION(1)/NTE(1)-3";
			dataRetrieved = terserDemonstrator.getData(terserExpression);
			System.out.printf("Observation group's 2nd NTE segment's second field using expression '%s' was: '%s' \n\n",terserExpression, dataRetrieved);
	        
			terserExpression = "/.OBSERVATION(1)/NTE-3";
			terserDemonstrator.setData(terserExpression,"This is our override value using the setter");
			System.out.printf("Setting the data for second repetition of the NTE segment and its third field\n",terserExpression, dataRetrieved);
	        
	        System.out.println("\nWill display our modified message below \n");
	        System.out.println(ourPipeParser.encode(orderResultsHl7Message));
			
		} catch (Exception e) {
			e.printStackTrace();
		}

	}

	public static String readHL7MessageFromFileAsString(String fileName)throws Exception
	  {
	    return new String(Files.readAllBytes(Paths.get(fileName)));
	  }
	
}

