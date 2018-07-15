package com.saravanansubramanian.hapihl7tutorial.parsers.custommodel.v25.message;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.v25.message.ADT_A01;
import ca.uhn.hl7v2.parser.DefaultModelClassFactory;
import ca.uhn.hl7v2.parser.ModelClassFactory;
import java.util.Arrays;
import com.saravanansubramanian.hapihl7tutorial.parsers.custommodel.v25.segment.ZPV;

public class ZDT_A01 extends ADT_A01 {
	
	// For this example, let's imagine an application that has a custom
    // patient visit segment called ZPV after the PID segment in an ADT^A01 message.
	
    // We will define the ZPI segment to have two custom fields:
    //   1. A non-repeating field (with data type ST) called "Custom Notes"
    //   2 - a non repeating field (also with data type ST) called "Custom Description"

	private static final long serialVersionUID = 1L;

	public ZDT_A01() throws HL7Exception {
		this(new DefaultModelClassFactory());
	}

	public ZDT_A01(ModelClassFactory factory) throws HL7Exception {
		super(factory);

		String[] segmentNamesInMessage = getNames();
		int indexOfPid = Arrays.asList(segmentNamesInMessage).indexOf("PID"); // look for PID segment
		int indexOfZSegment = indexOfPid + 1; // ZPV segment appears immediately after the PID segment

		Class<ZPV> type = ZPV.class;
		boolean required = true;
		boolean repeating = false;

		this.add(type, required, repeating, indexOfZSegment); //add this segment to the message payload
	}

	public ZPV getZPVSegment() throws HL7Exception {
		return getTyped("ZPV", ZPV.class);
	}

}
