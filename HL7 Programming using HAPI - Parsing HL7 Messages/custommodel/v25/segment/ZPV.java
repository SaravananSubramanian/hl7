package com.saravanansubramanian.hapihl7tutorial.parsers.custommodel.v25.segment;

import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.model.AbstractSegment;
import ca.uhn.hl7v2.model.Group;
import ca.uhn.hl7v2.model.Type;
import ca.uhn.hl7v2.model.v25.datatype.ST;
import ca.uhn.hl7v2.parser.ModelClassFactory;

public class ZPV extends AbstractSegment {

    private static final long serialVersionUID = 1;
	
    public ZPV(Group parent, ModelClassFactory factory) {
        super(parent, factory);
        init(factory);
     }

     private void init(ModelClassFactory factory) {
        try {
        	//initialize the segment with the two custom fields we have need to include
        	addFieldToSegment("Custom Location",true,0,100);
        	addFieldToSegment("Custom Description",true,0,100);
        } catch (HL7Exception e) {
            log.error("There was an error creating the custom segment ZPV.", e);
        }
     }

	private void addFieldToSegment(String fieldName, boolean isFieldMandatory, int numberOfFieldRepetitions, int fieldLength) throws HL7Exception {
		add(ST.class, isFieldMandatory, numberOfFieldRepetitions, fieldLength, new Object[]{ getMessage() }, fieldName);
	}

    public ST[] getCustomNotes() throws HL7Exception {
    	return getTypedStringTextFieldFromFieldIndexPosition(1);
    }
    
    public ST[] getCustomDescription() throws HL7Exception {
    	return getTypedStringTextFieldFromFieldIndexPosition(2);
    }

	private ST[] getTypedStringTextFieldFromFieldIndexPosition(int index) {
		return getTypedField(index,new ST[0]);
	}
    
    @Override
    protected Type createNewTypeWithoutReflection(int field) {
        return null;
    }

}
