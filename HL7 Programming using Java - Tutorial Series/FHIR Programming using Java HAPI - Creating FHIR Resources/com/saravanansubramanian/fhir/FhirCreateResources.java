package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.api.MethodOutcome;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import org.hl7.fhir.r4.model.*;

import java.time.LocalDate;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.Date;

/**
 * This tutorial demonstrates how to create FHIR resources using Java and the HAPI FHIR library.
 * We'll focus on creating a Patient resource, which is one of the most commonly used
 * resources in FHIR.
 */
public class FhirCreateResources {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Creating FHIR Resources Tutorial");
        System.out.println("================================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Create a new Patient resource
            System.out.println("Step 1: Creating a Patient resource locally...");
            Patient patient = new Patient();

            // Add an identifier
            patient.addIdentifier()
                    .setSystem("http://hospital.smarthealthit.org")
                    .setValue("123456");

            // Add a name
            patient.addName()
                    .setUse(HumanName.NameUse.OFFICIAL)
                    .setFamily("Doe")
                    .addGiven("John")
                    .addGiven("Adam")
                    .addPrefix("Mr.");

            // Set gender
            patient.setGender(Enumerations.AdministrativeGender.MALE);

            // Set birth date using java.time (thread-safe)
            LocalDate birthDate = LocalDate.parse("1980-01-01", DateTimeFormatter.ISO_LOCAL_DATE);
            patient.setBirthDate(Date.from(birthDate.atStartOfDay(ZoneId.systemDefault()).toInstant()));

            // Set active status
            patient.setActive(true);

            // Add address
            patient.addAddress()
                    .setUse(Address.AddressUse.HOME)
                    .setType(Address.AddressType.PHYSICAL)
                    .addLine("123 Main St")
                    .setCity("Metropolis")
                    .setState("NY")
                    .setPostalCode("10001")
                    .setCountry("USA");

            // Add contact information
            patient.addTelecom()
                    .setSystem(ContactPoint.ContactPointSystem.PHONE)
                    .setValue("555-123-4567")
                    .setUse(ContactPoint.ContactPointUse.HOME);

            patient.addTelecom()
                    .setSystem(ContactPoint.ContactPointSystem.EMAIL)
                    .setValue("john.doe@example.com")
                    .setUse(ContactPoint.ContactPointUse.WORK);

            System.out.println("Patient resource created locally:");
            if (patient.hasName()) {
                HumanName name = patient.getNameFirstRep();
                String givenNames = name.hasGiven()
                        ? String.join(" ", name.getGiven().stream().map(StringType::getValue).toList())
                        : "";
                System.out.println("  Name: " + name.getFamily() + ", " + givenNames);
            }
            System.out.println("  Gender: " + patient.getGender());
            System.out.println("  Birth Date: " + birthDate);
            System.out.println();

            // Step 2: Send the Patient to the FHIR server
            System.out.println("Step 2: Sending Patient resource to FHIR server...");
            MethodOutcome outcome = client.create()
                    .resource(patient)
                    .execute();

            // Check if the outcome has an ID before accessing it
            if (outcome.getId() == null) {
                System.err.println("Error: Failed to create patient - no ID returned from server.");
                return;
            }

            String patientId = outcome.getId().getIdPart();
            System.out.println();
            System.out.println("Patient created successfully on the FHIR server!");
            System.out.println("  Resource ID: " + patientId);
            System.out.println("  Resource URL: " + fhirServerUrl + "/Patient/" + patientId);
            System.out.println();

            // Step 3: Verify the creation by reading it back
            System.out.println("Step 3: Verifying creation by reading the patient back...");
            Patient retrievedPatient = client.read()
                    .resource(Patient.class)
                    .withId(patientId)
                    .execute();

            System.out.println();
            System.out.println("Retrieved Patient:");
            System.out.println("  ID: " + retrievedPatient.getId());

            if (retrievedPatient.hasName() && !retrievedPatient.getName().isEmpty()) {
                HumanName name = retrievedPatient.getNameFirstRep();
                String givenNames = name.hasGiven()
                        ? String.join(" ", name.getGiven().stream().map(StringType::getValue).toList())
                        : "";
                System.out.println("  Name: " + name.getFamily() + ", " + givenNames);
            }

            System.out.println("  Gender: " + retrievedPatient.getGender());
            System.out.println("  Active: " + retrievedPatient.getActive());

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
