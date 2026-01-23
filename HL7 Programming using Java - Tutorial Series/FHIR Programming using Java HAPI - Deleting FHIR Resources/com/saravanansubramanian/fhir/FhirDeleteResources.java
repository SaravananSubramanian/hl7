package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.api.MethodOutcome;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import ca.uhn.fhir.rest.server.exceptions.ResourceGoneException;
import ca.uhn.fhir.rest.server.exceptions.ResourceNotFoundException;
import org.hl7.fhir.r4.model.*;

import java.util.UUID;

/**
 * This tutorial demonstrates how to delete FHIR resources using Java and the HAPI FHIR library.
 * Deleting resources is an important part of managing healthcare data lifecycle.
 */
public class FhirDeleteResources {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Deleting FHIR Resources Tutorial");
        System.out.println("================================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Create a patient to delete
            System.out.println("Step 1: Creating a patient resource to delete...");
            System.out.println("-----------------------------------------------");

            Patient patient = new Patient();
            patient.addName()
                    .setFamily("ToBeDeleted")
                    .addGiven("Test");
            patient.setGender(Enumerations.AdministrativeGender.MALE);
            patient.setActive(true);

            MethodOutcome createOutcome = client.create()
                    .resource(patient)
                    .execute();

            // Check if the outcome has an ID before accessing it
            if (createOutcome.getId() == null) {
                System.err.println("Error: Failed to create patient - no ID returned from server.");
                return;
            }

            String patientId = createOutcome.getId().getIdPart();
            System.out.println("Patient created with ID: " + patientId);
            System.out.println();

            // Step 2: Delete the patient by ID
            System.out.println("Step 2: Deleting the Patient resource by ID...");
            System.out.println("---------------------------------------------");

            client.delete()
                    .resourceById(new IdType("Patient", patientId))
                    .execute();

            System.out.println("Patient/" + patientId + " deleted successfully.");
            System.out.println();

            // Step 3: Verify deletion by attempting to read
            System.out.println("Step 3: Verifying deletion by attempting to read the patient...");
            System.out.println("--------------------------------------------------------------");

            try {
                Patient deletedPatient = client.read()
                        .resource(Patient.class)
                        .withId(patientId)
                        .execute();
                System.out.println("Warning: Resource still exists after deletion!");
            } catch (ResourceNotFoundException | ResourceGoneException e) {
                System.out.println("Verified: The patient resource has been deleted (404/410 response).");
            }

            System.out.println();

            // Step 4: Conditional delete
            System.out.println("Step 4: Demonstrating Conditional Delete...");
            System.out.println("-----------------------------------------");

            // Create a patient with a unique identifier
            String uniqueIdentifier = "DELETE-TEST-" + UUID.randomUUID().toString().substring(0, 8);

            Patient patientForConditionalDelete = new Patient();
            patientForConditionalDelete.addIdentifier()
                    .setSystem("http://example.org/mrn")
                    .setValue(uniqueIdentifier);
            patientForConditionalDelete.addName()
                    .setFamily("ConditionalDelete")
                    .addGiven("Test");

            MethodOutcome conditionalCreateOutcome = client.create()
                    .resource(patientForConditionalDelete)
                    .execute();

            System.out.println("Created patient with identifier: " + uniqueIdentifier);
            String conditionalPatientId = (conditionalCreateOutcome.getId() != null)
                    ? conditionalCreateOutcome.getId().getIdPart()
                    : "Unknown";
            System.out.println("Patient ID: " + conditionalPatientId);

            // Perform conditional delete based on identifier
            client.delete()
                    .resourceConditionalByType(Patient.class)
                    .where(Patient.IDENTIFIER.exactly().systemAndCode("http://example.org/mrn", uniqueIdentifier))
                    .execute();

            System.out.println("Conditional delete completed for identifier: " + uniqueIdentifier);
            System.out.println();

            System.out.println("Important Notes about FHIR Deletions:");
            System.out.println("-------------------------------------");
            System.out.println("1. Some FHIR servers may not support deletion (check CapabilityStatement).");
            System.out.println("2. Deleted resources may still be retrievable via history.");
            System.out.println("3. Conditional deletes should be used carefully to avoid unintended data loss.");
            System.out.println("4. Consider using logical deletion (setting active=false) instead of physical deletion.");

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
