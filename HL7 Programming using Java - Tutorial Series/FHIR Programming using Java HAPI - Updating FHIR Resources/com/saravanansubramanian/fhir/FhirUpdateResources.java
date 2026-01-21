package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.api.MethodOutcome;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import org.hl7.fhir.r4.model.*;

import java.util.UUID;

/**
 * This tutorial demonstrates how to update FHIR resources using Java and the HAPI FHIR library.
 * Updating resources is essential for maintaining accurate healthcare data.
 */
public class FhirUpdateResources {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Updating FHIR Resources Tutorial");
        System.out.println("================================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Create a patient to update
            System.out.println("Step 1: Creating a patient resource to update...");
            System.out.println("-----------------------------------------------");

            Patient patient = new Patient();
            patient.addName()
                    .setFamily("OriginalLastName")
                    .addGiven("John");
            patient.setGender(Enumerations.AdministrativeGender.MALE);
            patient.setActive(true);

            MethodOutcome createOutcome = client.create()
                    .resource(patient)
                    .execute();

            String patientId = createOutcome.getId().getIdPart();
            System.out.println("Patient created with ID: " + patientId);
            System.out.println("Original Name: " + patient.getNameFirstRep().getFamily() + ", " +
                    String.join(" ", patient.getNameFirstRep().getGiven().stream()
                            .map(StringType::getValue).toList()));
            System.out.println();

            // Step 2: Update the patient's family name
            System.out.println("Step 2: Updating the patient's family name...");
            System.out.println("-------------------------------------------");

            // Read the patient first to get the current version
            Patient patientToUpdate = client.read()
                    .resource(Patient.class)
                    .withId(patientId)
                    .execute();

            // Modify the family name
            patientToUpdate.getNameFirstRep().setFamily("UpdatedLastName");

            // Execute the update
            MethodOutcome updateOutcome = client.update()
                    .resource(patientToUpdate)
                    .execute();

            System.out.println("Patient updated successfully!");
            System.out.println("New Version ID: " + updateOutcome.getId().getVersionIdPart());
            System.out.println();

            // Step 3: Add contact information
            System.out.println("Step 3: Adding contact information...");
            System.out.println("-----------------------------------");

            // Read the latest version
            Patient patientWithNewVersion = client.read()
                    .resource(Patient.class)
                    .withId(patientId)
                    .execute();

            // Add a phone number
            patientWithNewVersion.addTelecom()
                    .setSystem(ContactPoint.ContactPointSystem.PHONE)
                    .setValue("555-987-6543")
                    .setUse(ContactPoint.ContactPointUse.HOME);

            MethodOutcome contactOutcome = client.update()
                    .resource(patientWithNewVersion)
                    .execute();

            System.out.println("Contact added: " + patientWithNewVersion.getTelecomFirstRep().getValue());
            System.out.println();

            // Step 4: Conditional update (by identifier)
            System.out.println("Step 4: Conditional Update (by identifier)...");
            System.out.println("------------------------------------------");

            // Read the latest version
            Patient patientForConditional = client.read()
                    .resource(Patient.class)
                    .withId(patientId)
                    .execute();

            // Add a unique identifier
            String uniqueIdentifier = "UPDATE-TEST-" + UUID.randomUUID().toString().substring(0, 8);
            patientForConditional.addIdentifier()
                    .setSystem("http://example.org/mrn")
                    .setValue(uniqueIdentifier);

            // Update to add the identifier
            client.update()
                    .resource(patientForConditional)
                    .execute();

            System.out.println("Added identifier: " + uniqueIdentifier);

            // Now do a conditional update using the identifier
            Patient patientForConditionalUpdate = client.read()
                    .resource(Patient.class)
                    .withId(patientId)
                    .execute();

            patientForConditionalUpdate.getNameFirstRep().setFamily("ConditionallyUpdatedName");

            MethodOutcome conditionalOutcome = client.update()
                    .resource(patientForConditionalUpdate)
                    .conditional()
                    .where(Patient.IDENTIFIER.exactly().systemAndCode("http://example.org/mrn", uniqueIdentifier))
                    .execute();

            System.out.println("Conditional update completed!");
            System.out.println("Final Name: ConditionallyUpdatedName");
            System.out.println();

            // Step 5: Verify all updates
            System.out.println("Step 5: Verifying all updates...");
            System.out.println("-------------------------------");

            Patient finalPatient = client.read()
                    .resource(Patient.class)
                    .withId(patientId)
                    .execute();

            System.out.println("Final Patient State:");
            System.out.println("  ID: " + finalPatient.getIdElement().getIdPart());
            System.out.println("  Name: " + finalPatient.getNameFirstRep().getFamily());
            System.out.println("  Active: " + finalPatient.getActive());

            if (finalPatient.hasTelecom()) {
                System.out.println("  Phone: " + finalPatient.getTelecomFirstRep().getValue());
            }

            if (finalPatient.hasIdentifier()) {
                System.out.println("  Identifier: " + finalPatient.getIdentifierFirstRep().getValue());
            }

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
