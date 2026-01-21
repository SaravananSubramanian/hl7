package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import org.hl7.fhir.r4.model.*;

/**
 * This tutorial demonstrates how to search for FHIR resources using Java and the HAPI FHIR library.
 * Searching is a fundamental operation for retrieving specific data from a FHIR server.
 */
public class FhirSearchResources {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Searching FHIR Resources Tutorial");
        System.out.println("=================================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Basic search by family name
            System.out.println("Step 1: Basic Search by Family Name...");
            System.out.println("-------------------------------------");

            Bundle smithPatients = client.search()
                    .forResource(Patient.class)
                    .where(Patient.FAMILY.matches().value("Smith"))
                    .count(5)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + smithPatients.getTotal() + " patient(s) with family name 'Smith':");
            printPatients(smithPatients);
            System.out.println();

            // Step 2: Advanced search with multiple criteria
            System.out.println("Step 2: Advanced Search with Multiple Criteria...");
            System.out.println("-----------------------------------------------");

            Bundle maleSmithPatients = client.search()
                    .forResource(Patient.class)
                    .where(Patient.FAMILY.matches().value("Smith"))
                    .and(Patient.GENDER.exactly().code("male"))
                    .count(5)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + maleSmithPatients.getTotal() + " male patient(s) named 'Smith':");
            printPatients(maleSmithPatients);
            System.out.println();

            // Step 3: Search with date range
            System.out.println("Step 3: Search with Date Range...");
            System.out.println("-------------------------------");

            Bundle dateRangePatients = client.search()
                    .forResource(Patient.class)
                    .where(Patient.BIRTHDATE.afterOrEquals().day("1980-01-01"))
                    .and(Patient.BIRTHDATE.beforeOrEquals().day("1990-12-31"))
                    .count(5)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + dateRangePatients.getTotal() + " patient(s) born between 1980 and 1990:");
            printPatients(dateRangePatients);
            System.out.println();

            // Step 4: Paginated search results
            System.out.println("Step 4: Handling Paginated Search Results...");
            System.out.println("------------------------------------------");

            Bundle paginatedResult = client.search()
                    .forResource(Patient.class)
                    .count(3)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Page 1: Showing " + paginatedResult.getEntry().size() +
                    " of " + paginatedResult.getTotal() + " total patients");
            printPatients(paginatedResult);

            // Check if there's a next page
            if (paginatedResult.getLink(Bundle.LINK_NEXT) != null) {
                System.out.println();
                System.out.println("Fetching next page...");

                Bundle nextPage = client.loadPage()
                        .next(paginatedResult)
                        .execute();

                System.out.println("Page 2: Showing " + nextPage.getEntry().size() + " patients");
                printPatients(nextPage);
            }

            System.out.println();

            // Step 5: Search for Observations
            System.out.println("Step 5: Searching for Observations...");
            System.out.println("-----------------------------------");

            Bundle observations = client.search()
                    .forResource(Observation.class)
                    .where(Observation.CODE.exactly().systemAndCode("http://loinc.org", "8867-4"))
                    .count(3)
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + observations.getTotal() + " heart rate observations:");

            for (Bundle.BundleEntryComponent entry : observations.getEntry()) {
                Observation obs = (Observation) entry.getResource();
                System.out.println("  - ID: " + obs.getIdElement().getIdPart() + ", Status: " + obs.getStatus());

                if (obs.hasValueQuantity()) {
                    Quantity value = obs.getValueQuantity();
                    System.out.println("    Value: " + value.getValue() + " " + value.getUnit());
                }
            }

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }

    private static void printPatients(Bundle bundle) {
        for (Bundle.BundleEntryComponent entry : bundle.getEntry()) {
            Patient patient = (Patient) entry.getResource();
            String name = "Unknown";

            if (patient.hasName() && !patient.getName().isEmpty()) {
                HumanName humanName = patient.getNameFirstRep();
                name = humanName.getFamily() + ", " +
                        String.join(" ", humanName.getGiven().stream()
                                .map(StringType::getValue).toList());
            }

            System.out.println("  - " + name +
                    " (ID: " + patient.getIdElement().getIdPart() +
                    ", Gender: " + patient.getGender() +
                    ", DOB: " + (patient.hasBirthDate() ? patient.getBirthDate() : "N/A") + ")");
        }
    }
}
