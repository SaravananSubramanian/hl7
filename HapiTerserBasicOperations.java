package com.saravanansubramanian.hapihl7tutorial.tersers;

import java.nio.file.Files;
import java.nio.file.Paths;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.parser.PipeParser;
import ca.uhn.hl7v2.util.Terser;

public class HapiTerserBasicOperations {

	public static void main(String[] args) {
		
		try {
			
			//see my GitHub page for this file
			String messageString = readHL7MessageFromFileAsString("C:\\HL7TestInputFiles\\FileWithOrderResultMessage.txt");
			
			// instantiate a PipeParser, which handles the "traditional or default encoding"
			PipeParser ourPipeParser = new PipeParser();
						
			// parse the message string into a Java message object
			Message orderResultsHl7Message = ourPipeParser.parse(messageString);
			
			//create a terser object instance by wrapping it around the message object
			Terser terser = new Terser(orderResultsHl7Message);
			
			//now, let us do various operations on the message
			
			exampleOfAbsolutePositioning(terser);
	        
			exampleOfRelativePositioning(terser);
	        
	        exampleOfRetrievingSpecificComponent(terser);
	        
	        exampleOfUsingWildcardExpression(terser);
	        
	        exampleOfAnotherWildcardExpression(terser);
	        
	        exampleOfRetrievingSpecificRepetitionOfSegment(terser);
	        
	        exampleOfSettingDataUsingTerser(terser);
	        
	        //Print the updated message to the display
	        //You should see the message value updated as a result of the last operation
	        System.out.println("\nWill display our modified message below \n");
	        System.out.println(ourPipeParser.encode(orderResultsHl7Message));
			
		} catch (Exception e) {
			e.printStackTrace();
		}

	}

	private static void exampleOfAbsolutePositioning(Terser terser) throws HL7Exception {
		System.out.printf("Using absolute path reference to retrieve an entire field...\n");
		String terserExpression = "MSH-6";
		String value = terser.get(terserExpression);
		System.out.printf("Field 6 of MSH segment using expression '%s' was: '%s' \n\n",terserExpression, value);
	}
	
	private static void exampleOfRelativePositioning(Terser terser) throws HL7Exception {
		System.out.printf("Using relative positioning to retrieve an entire field...\n");
		String expression = "/.MSH-6";
		String value = terser.get(expression);
		System.out.printf("Field 6 of MSH segment using expression '%s' was: '%s' \n\n",expression, value);
	}
	
	private static void exampleOfRetrievingSpecificComponent(Terser terser) throws HL7Exception {
		System.out.printf("Retrieving a specific component value inside a field...\n");
		String expression = "/.PID-5-2";
		String value = terser.get(expression);
		System.out.printf("Field 5 and Component 2 of the MSH segment using expression '%s' was: '%s' \n\n",expression, value);
	}
	
	private static void exampleOfUsingWildcardExpression(Terser terser) throws HL7Exception {
		System.out.printf("Retrieving a specific component value inside a field using wild card *...\n");
		String expression = "/*ID-5-2";
		String value = terser.get(expression);
		System.out.printf("Field 5 and Component 2 of the MSH segment using expression '%s' was: '%s' \n\n",expression, value);
	}
	
	private static void exampleOfAnotherWildcardExpression(Terser terser) throws HL7Exception {
		System.out.printf("Retrieving a specific component value inside a field using another wild card ?...\n");
		String expression = "/P?D-5-2";
		String value = terser.get(expression);
		System.out.printf("Field 5 and Component 2 of the MSH segment using expression '%s' was: '%s' \n\n",expression, value);
	}
	
	private static void exampleOfRetrievingSpecificRepetitionOfSegment(Terser terser) throws HL7Exception {
		System.out.printf("Retrieving a specific repetition of a segment (2nd segment of NTE here)...\n");
		System.out.printf("and the 3rd field inside it...\n");
		String expression = "NTE(1)-3";
		String value = terser.get(expression);
		System.out.printf("Third field of the second segment of NTE using expression '%s' was: '%s' \n\n",expression, value);
	}
	
	private static void exampleOfSettingDataUsingTerser(Terser terser) throws HL7Exception {
		System.out.printf("Example 7. Setting the data for second repetition of the NTE segment and its third field... \n");
		terser.set("NTE(1)-3","This is our override value using the setter"); //this works correctly
		System.out.println("Set value successfully...\n");
	}
	
	private static String readHL7MessageFromFileAsString(String fileName)throws Exception
	  {
	    return new String(Files.readAllBytes(Paths.get(fileName)));
	  }

}
