package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import org.hl7.fhir.r4.model.CapabilityStatement;

/**
 * This tutorial demonstrates how to set up your development environment for FHIR programming
 * using Java and the HAPI FHIR library. This is the first step in working with FHIR
 * resources programmatically.
 *
 * Prerequisites:
 * - Java Development Kit (JDK) 17+ installed
 * - Apache Maven installed
 *
 * For testing, you can use a public FHIR test server like:
 * - http://hapi.fhir.org/baseR4
 * - https://server.fire.ly/r4
 */
public class FhirSetupEnvironment {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        // For testing, you can use a public test server like http://hapi.fhir.org/baseR4
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Environment Setup Tutorial");
        System.out.println("==========================================");
        System.out.println();

        try {
            // Step 1: Create a FHIR context for R4
            System.out.println("Step 1: Creating FHIR Context for R4...");
            FhirContext ctx = FhirContext.forR4();
            System.out.println("FHIR Context created successfully!");
            System.out.println();

            // Step 2: Create a FHIR client
            System.out.println("Step 2: Creating FHIR Client...");
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);
            System.out.println("FHIR Client created for server: " + fhirServerUrl);
            System.out.println();

            // Step 3: Test the connection by retrieving the capability statement
            System.out.println("Step 3: Testing connection by retrieving server capability statement...");
            CapabilityStatement capabilityStatement = client
                    .capabilities()
                    .ofType(CapabilityStatement.class)
                    .execute();

            if (capabilityStatement != null) {
                System.out.println();
                System.out.println("Connection successful!");
                System.out.println("Server Name: " +
                    (capabilityStatement.hasSoftware() ? capabilityStatement.getSoftware().getName() : "Unknown"));
                System.out.println("Server Version: " +
                    (capabilityStatement.hasSoftware() ? capabilityStatement.getSoftware().getVersion() : "Unknown"));
                System.out.println("FHIR Version: " + capabilityStatement.getFhirVersion());
                System.out.println("Publisher: " +
                    (capabilityStatement.hasPublisher() ? capabilityStatement.getPublisher() : "Unknown"));
                System.out.println();

                // Display supported resource types
                if (capabilityStatement.hasRest() && !capabilityStatement.getRest().isEmpty()) {
                    CapabilityStatement.CapabilityStatementRestComponent rest = capabilityStatement.getRestFirstRep();
                    int resourceCount = rest.getResource().size();
                    System.out.println("Supported Resources (" + resourceCount + " types):");

                    int count = 0;
                    for (CapabilityStatement.CapabilityStatementRestResourceComponent resource : rest.getResource()) {
                        if (count < 10) { // Show first 10 resources
                            System.out.println("  - " + resource.getType());
                        }
                        count++;
                    }

                    if (count > 10) {
                        System.out.println("  ... and " + (count - 10) + " more");
                    }
                }
            }

        } catch (Exception e) {
            System.err.println("Error connecting to FHIR server: " + e.getMessage());
            System.out.println();
            System.out.println("Make sure the FHIR server URL is correct and the server is accessible.");
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
