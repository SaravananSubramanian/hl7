package com.saravanansubramanian.hapihl7tutorial.tersers;

import java.nio.file.Files;
import java.nio.file.Paths;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.parser.PipeParser;
import ca.uhn.hl7v2.util.Terser;

public class HapiTerserBasicOperations {

	public static void main(String[] args) {

		try {

			// see my GitHub page for this file
			String messageString = readHL7MessageFromFileAsString(
					"C:\\HL7TestInputFiles\\FileWithObservationResultMessage.txt");

			// instantiate a PipeParser, which handles the normal HL7 encoding
			PipeParser ourPipeParser = new PipeParser();

			// parse the message string into a Java message object
			Message orderResultsHl7Message = ourPipeParser.parse(messageString);

			// create a terser object instance by wrapping it around the message object
			Terser terser = new Terser(orderResultsHl7Message);

			// now, let us do various operations on the message
			OurTerserHelper terserHelper = new OurTerserHelper(terser);

			String terserExpression = "MSH-6";
			String dataRetrieved = terserHelper.getData(terserExpression);
			System.out.printf("Field 6 of MSH segment using expression '%s' was: '%s' \n\n", terserExpression,dataRetrieved);

			terserExpression = "/.PID-5-2"; // notice the /. to indicate relative position to root node
			dataRetrieved = terserHelper.getData(terserExpression);
			System.out.printf("Field 5 and Component 2 of the PID segment using expression '%s' was: '%s' \n\n", terserExpression, dataRetrieved);

			terserExpression = "/.*ID-5-2";
			dataRetrieved = terserHelper.getData(terserExpression);
			System.out.printf("Field 5 and Component 2 of the PID segment using wildcard-based expression '%s' was: '%s' \n\n",terserExpression, dataRetrieved);

			terserExpression = "/.P?D-5-2";
			dataRetrieved = terserHelper.getData(terserExpression);
			System.out.printf("Field 5 and Component 2 of the PID segment using another wildcard-based expression '%s' was: '%s' \n\n",terserExpression, dataRetrieved);

			terserExpression = "/.PV1-9(1)-1"; // note: field repetitions are zero-indexed
			dataRetrieved = terserHelper.getData(terserExpression);
			System.out.printf("2nd repetition of Field 9 and Component 1 for it in the PV1 segment using expression '%s' was: '%s' \n\n",terserExpression, dataRetrieved);

		} catch (Exception e) {
			e.printStackTrace();
		}

	}

	public static String readHL7MessageFromFileAsString(String fileName) throws Exception {
		return new String(Files.readAllBytes(Paths.get(fileName)));
	}

}