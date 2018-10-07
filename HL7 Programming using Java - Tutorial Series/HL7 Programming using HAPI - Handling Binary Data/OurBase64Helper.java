package com.saravanansubramanian.hapihl7tutorial.handlingbinarydata;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.nio.file.Files;
import org.apache.commons.codec.binary.Base64;

public class OurBase64Helper {

	public String ConvertToBase64String(File inputFile) throws IOException
    {
        if (!inputFile.exists())
            throw new FileNotFoundException(String.format("The specified input file '%s' does not exist",inputFile.getAbsoluteFile()));

        return new String(Base64.encodeBase64(Files.readAllBytes(inputFile.toPath())));
    }

    public byte[] ConvertFromBase64String(byte[] base64EncodedByteData)
    {
        if (base64EncodedByteData == null)
            throw new IllegalArgumentException("You must supply byte data for Base64 decoding operation");
        
        String decodedByteData = Base64.encodeBase64String(base64EncodedByteData);
        return Base64.decodeBase64(decodedByteData);
    }

    public byte[] ConvertFromBase64String(String base64EncodedString)
    {
        if (base64EncodedString == null || base64EncodedString.length() == 0)
        	throw new IllegalArgumentException("You must supply byte string for Base64 decoding operation");

        return Base64.decodeBase64(base64EncodedString);
    }
}
