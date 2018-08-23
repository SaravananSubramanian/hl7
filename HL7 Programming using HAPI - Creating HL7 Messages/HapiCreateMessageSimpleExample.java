package com.saravanansubramanian.hapihl7tutorial.create;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.v24.message.ADT_A01;
import ca.uhn.hl7v2.parser.Parser;

public class HapiCreateMessageSimpleExample {
	
	// In HAPI, almost all things revolve around a context object
	private static HapiContext context = new DefaultHapiContext();

	public static void main(String[] args) throws Exception {

		try {

			// create the HL7 message
			// this AdtMessageFactory class is not from HAPI but my own wrapper
			System.out.println("Creating ADT A01 message...");
			ADT_A01 adtMessage = (ADT_A01) AdtMessageFactory.createMessage("A01");

			// create these parsers for file operations
			Parser pipeParser = context.getPipeParser();
			Parser xmlParser = context.getXMLParser();

			// print out the message that we constructed
			System.out.println("Message was constructed successfully..." + "\n");
			System.out.println(pipeParser.encode(adtMessage));

			// serialize the message to pipe delimited output file
			writeMessageToFile(pipeParser, adtMessage, "testPipeDelimitedOutputFile.txt");

			// serialize the message to XML format output file
			writeMessageToFile(xmlParser, adtMessage, "testXmlOutputFile.xml");
			
			//you can print out the message structure using a convenient helper method on the message class
			System.out.println("Printing message structure to console...");
			System.out.println(adtMessage.printStructure());

		} catch (Exception e) {
			e.printStackTrace();
		} 
	}

	private static void writeMessageToFile(Parser parser, ADT_A01 adtMessage, String outputFilename)
			throws IOException, FileNotFoundException, HL7Exception {
		OutputStream outputStream = null;
		try {

			// Remember that the file may not show special delimiter characters when using
			// plain text editor
			File file = new File(outputFilename);

			// quick check to create the file before writing if it does not exist already
			if (!file.exists()) {
				file.createNewFile();
			}

			System.out.println("Serializing message to file...");
			outputStream = new FileOutputStream(file);
			outputStream.write(parser.encode(adtMessage).getBytes());
			outputStream.flush();

			System.out.printf("Message serialized to file '%s' successfully", file);
			System.out.println("\n");
		} finally {
			if (outputStream != null) {
				outputStream.close();
			}
		}
	}

}
