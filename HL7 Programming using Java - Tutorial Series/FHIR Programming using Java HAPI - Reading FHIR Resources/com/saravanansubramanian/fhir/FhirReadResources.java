package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import ca.uhn.fhir.rest.server.exceptions.ResourceNotFoundException;
import org.hl7.fhir.r4.model.Bundle;
import org.hl7.fhir.r4.model.HumanName;
import org.hl7.fhir.r4.model.Patient;
import org.hl7.fhir.r4.model.Resource;
import org.hl7.fhir.r4.model.StringType;

/**
 * This tutorial demonstrates how to read FHIR resources using Java and the HAPI FHIR library.
 * Reading resources is a fundamental operation that allows you to retrieve healthcare data
 * from a FHIR server.
 */
public class FhirReadResources {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Reading FHIR Resources Tutorial");
        System.out.println("===============================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Search for any patient to get a valid ID
            System.out.println("Step 1: Searching for a Patient to read...");
            System.out.println("------------------------------------------");

            Bundle searchResult = client.search()
                    .forResource(Patient.class)
                    .count(1)
                    .returnBundle(Bundle.class)
                    .execute();

            if (searchResult.hasEntry() && !searchResult.getEntry().isEmpty()) {
                Resource resource = searchResult.getEntry().get(0).getResource();

                // Safe type check using instanceof
                if (resource instanceof Patient firstPatient) {
                    String patientId = firstPatient.getIdElement().getIdPart();

                    System.out.println("Found patient ID: " + patientId);
                    System.out.println();

                    // Step 2: Read the specific patient by ID
                    System.out.println("Step 2: Reading Patient by ID...");
                    System.out.println("--------------------------------");

                    Patient patient = client.read()
                            .resource(Patient.class)
                            .withId(patientId)
                            .execute();

                    System.out.println("Patient Details:");
                    System.out.println("  ID: " + patient.getId());

                    if (patient.hasName() && !patient.getName().isEmpty()) {
                        HumanName name = patient.getNameFirstRep();
                        String givenNames = name.hasGiven()
                                ? String.join(" ", name.getGiven().stream().map(StringType::getValue).toList())
                                : "";
                        System.out.println("  Name: " + name.getFamily() + ", " + givenNames);
                    }

                    System.out.println("  Gender: " + patient.getGender());
                    System.out.println("  Birth Date: " + patient.getBirthDate());
                    System.out.println("  Active: " + patient.getActive());
                } else {
                    System.out.println("Unexpected resource type returned.");
                }

            } else {
                System.out.println("No patients found on the server.");
            }

            System.out.println();

            // Step 3: Search for Patients by family name
            System.out.println("Step 3: Searching for Patients by family name...");
            System.out.println("-----------------------------------------------");

            Bundle smithPatients = client.search()
                    .forResource(Patient.class)
                    .where(Patient.FAMILY.matches().value("Smith"))
                    .count(5)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + smithPatients.getTotal() + " patient(s) with family name 'Smith':");

            for (Bundle.BundleEntryComponent entry : smithPatients.getEntry()) {
                Resource entryResource = entry.getResource();

                // Safe type check using instanceof with pattern matching
                if (entryResource instanceof Patient patient) {
                    if (patient.hasName() && !patient.getName().isEmpty()) {
                        HumanName name = patient.getNameFirstRep();
                        String givenNames = name.hasGiven()
                                ? String.join(" ", name.getGiven().stream().map(StringType::getValue).toList())
                                : "";
                        System.out.println("  - " + name.getFamily() + ", " + givenNames +
                                " (ID: " + patient.getIdElement().getIdPart() + ")");
                    } else {
                        System.out.println("  - (No name) ID: " + patient.getIdElement().getIdPart());
                    }
                }
            }

            System.out.println();

            // Step 4: Error handling for non-existent resources
            System.out.println("Step 4: Handling errors when reading non-existent resources...");
            System.out.println("-------------------------------------------------------------");

            try {
                Patient nonExistent = client.read()
                        .resource(Patient.class)
                        .withId("non-existent-id-12345")
                        .execute();
            } catch (ResourceNotFoundException e) {
                System.out.println("Expected error caught: " + e.getMessage());
                System.out.println("  The specified patient could not be found (404 Not Found).");
            }

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
