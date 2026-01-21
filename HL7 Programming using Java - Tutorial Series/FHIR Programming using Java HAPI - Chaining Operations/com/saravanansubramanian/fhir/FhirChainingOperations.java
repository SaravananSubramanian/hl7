package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import org.hl7.fhir.r4.model.*;

/**
 * This tutorial demonstrates how to chain FHIR operations using Java and the HAPI FHIR library.
 * Chaining allows you to perform complex queries that span across multiple related resources.
 */
public class FhirChainingOperations {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Chaining FHIR Operations Tutorial");
        System.out.println("=================================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Basic chained query - Find Observations for a Patient
            System.out.println("Step 1: Basic Chained Query - Find Observations for a Patient...");
            System.out.println("---------------------------------------------------------------");

            // First, find a patient to work with
            Bundle patientSearch = client.search()
                    .forResource(Patient.class)
                    .count(1)
                    .returnBundle(Bundle.class)
                    .execute();

            if (patientSearch.hasEntry() && !patientSearch.getEntry().isEmpty()) {
                Patient patient = (Patient) patientSearch.getEntry().get(0).getResource();
                String patientId = patient.getIdElement().getIdPart();

                System.out.println("Using Patient ID: " + patientId);
                System.out.println();

                // Search for Observations linked to this Patient
                Bundle observations = client.search()
                        .forResource(Observation.class)
                        .where(Observation.SUBJECT.hasId("Patient/" + patientId))
                        .count(5)
                        .returnBundle(Bundle.class)
                        .execute();

                System.out.println("Found " + observations.getTotal() + " Observations for this patient:");

                for (Bundle.BundleEntryComponent entry : observations.getEntry()) {
                    Observation obs = (Observation) entry.getResource();
                    System.out.println("  - Observation ID: " + obs.getIdElement().getIdPart() +
                            ", Status: " + obs.getStatus());

                    if (obs.hasCode() && obs.getCode().hasCoding()) {
                        Coding coding = obs.getCode().getCodingFirstRep();
                        System.out.println("    Code: " +
                                (coding.hasDisplay() ? coding.getDisplay() : coding.getCode()));
                    }
                }
            } else {
                System.out.println("No patients found on the server.");
            }

            System.out.println();

            // Step 2: Chained search with patient criteria
            System.out.println("Step 2: Chained Search - Find Encounters for Patients named Smith...");
            System.out.println("-------------------------------------------------------------------");

            Bundle encounters = client.search()
                    .forResource(Encounter.class)
                    .where(Encounter.SUBJECT.hasChainedProperty(
                            Patient.FAMILY.matches().value("Smith")))
                    .count(5)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + encounters.getTotal() + " Encounters for patients named Smith:");

            for (Bundle.BundleEntryComponent entry : encounters.getEntry()) {
                Encounter encounter = (Encounter) entry.getResource();
                System.out.println("  - Encounter ID: " + encounter.getIdElement().getIdPart() +
                        ", Status: " + encounter.getStatus());
            }

            System.out.println();

            // Step 3: Handling paginated chained results
            System.out.println("Step 3: Handling Paginated Chained Results...");
            System.out.println("--------------------------------------------");

            Bundle resultSet = client.search()
                    .forResource(Observation.class)
                    .count(3)
                    .returnBundle(Bundle.class)
                    .execute();

            int pageNum = 1;
            int totalProcessed = 0;
            int maxPages = 3; // Limit for demo purposes

            while (resultSet != null && pageNum <= maxPages) {
                System.out.println("Page " + pageNum + ": " + resultSet.getEntry().size() + " results");

                for (Bundle.BundleEntryComponent entry : resultSet.getEntry()) {
                    Observation obs = (Observation) entry.getResource();
                    System.out.println("  - " + obs.getIdElement().getIdPart() + ": " + obs.getStatus());
                    totalProcessed++;
                }

                // Get the next page
                if (resultSet.getLink(Bundle.LINK_NEXT) != null && pageNum < maxPages) {
                    resultSet = client.loadPage()
                            .next(resultSet)
                            .execute();
                    pageNum++;
                } else {
                    break;
                }
            }

            System.out.println();
            System.out.println("Total observations processed: " + totalProcessed);
            System.out.println();

            // Step 4: Using _include to get related resources
            System.out.println("Step 4: Using _include to Get Related Resources...");
            System.out.println("-------------------------------------------------");

            Bundle includeSearch = client.search()
                    .forResource(Observation.class)
                    .include(Observation.INCLUDE_PATIENT)
                    .count(3)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + includeSearch.getEntry().size() + " resources (including related):");

            for (Bundle.BundleEntryComponent entry : includeSearch.getEntry()) {
                Resource resource = entry.getResource();
                System.out.println("  - " + resource.fhirType() + "/" + resource.getIdElement().getIdPart());
            }

            System.out.println();
            System.out.println("Chaining Best Practices:");
            System.out.println("------------------------");
            System.out.println("1. Chained queries reduce the number of API calls needed.");
            System.out.println("2. Use _include and _revinclude to fetch related resources efficiently.");
            System.out.println("3. Always handle pagination for large result sets.");
            System.out.println("4. Be mindful of server-side performance with complex chains.");

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
