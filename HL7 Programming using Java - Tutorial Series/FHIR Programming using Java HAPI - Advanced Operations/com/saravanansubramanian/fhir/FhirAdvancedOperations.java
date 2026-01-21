package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import org.hl7.fhir.r4.model.*;

import java.util.Date;
import java.util.List;
import java.util.UUID;

/**
 * This tutorial demonstrates advanced FHIR operations using Java and the HAPI FHIR library.
 * Topics covered include transactions, auditing, and security best practices.
 */
public class FhirAdvancedOperations {

    public static void main(String[] args) {
        // Replace with your FHIR server base URL
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Advanced Operations Tutorial");
        System.out.println("============================================");
        System.out.println();

        try {
            // Initialize FHIR context and client
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Creating FHIR Transactions
            System.out.println("Step 1: FHIR Transactions (Bundle Operations)...");
            System.out.println("----------------------------------------------");

            // Create a transaction bundle
            Bundle transactionBundle = new Bundle();
            transactionBundle.setType(Bundle.BundleType.TRANSACTION);

            // Generate a temporary UUID for the patient
            String patientUuid = "urn:uuid:" + UUID.randomUUID();

            // Add a Patient to the transaction
            Patient patient = new Patient();
            patient.addName()
                    .setFamily("TransactionTest")
                    .addGiven("John");
            patient.setGender(Enumerations.AdministrativeGender.MALE);

            transactionBundle.addEntry()
                    .setFullUrl(patientUuid)
                    .setResource(patient)
                    .getRequest()
                        .setMethod(Bundle.HTTPVerb.POST)
                        .setUrl("Patient");

            // Add an Observation linked to the Patient
            Observation observation = new Observation();
            observation.setStatus(Observation.ObservationStatus.FINAL);
            observation.setCode(new CodeableConcept()
                    .addCoding(new Coding()
                            .setSystem("http://loinc.org")
                            .setCode("8867-4")
                            .setDisplay("Heart rate")));
            observation.setSubject(new Reference(patientUuid));
            observation.setValue(new Quantity()
                    .setValue(72)
                    .setUnit("beats/minute")
                    .setSystem("http://unitsofmeasure.org")
                    .setCode("/min"));

            transactionBundle.addEntry()
                    .setResource(observation)
                    .getRequest()
                        .setMethod(Bundle.HTTPVerb.POST)
                        .setUrl("Observation");

            System.out.println("Executing transaction with " + transactionBundle.getEntry().size() + " entries...");

            Bundle transactionResponse = client.transaction()
                    .withBundle(transactionBundle)
                    .execute();

            System.out.println("Transaction completed successfully!");
            System.out.println("Response contains " + transactionResponse.getEntry().size() + " entries:");

            for (Bundle.BundleEntryComponent entry : transactionResponse.getEntry()) {
                if (entry.getResponse() != null) {
                    System.out.println("  - " + entry.getResponse().getLocation() +
                            " (Status: " + entry.getResponse().getStatus() + ")");
                }
            }

            System.out.println();

            // Step 2: Creating Audit Events
            System.out.println("Step 2: Creating Audit Events...");
            System.out.println("-------------------------------");

            AuditEvent auditEvent = new AuditEvent();
            auditEvent.setType(new Coding()
                    .setSystem("http://terminology.hl7.org/CodeSystem/audit-event-type")
                    .setCode("rest")
                    .setDisplay("RESTful Operation"));
            auditEvent.setAction(AuditEvent.AuditEventAction.C);
            auditEvent.setOutcome(AuditEvent.AuditEventOutcome._0);
            auditEvent.setRecorded(new Date());

            // Add agent information
            AuditEvent.AuditEventAgentComponent agent = auditEvent.addAgent();
            agent.setWho(new Reference("Practitioner/example"));
            agent.setRequestor(true);

            // Add source information
            auditEvent.getSource().setObserver(new Reference("Device/software-system"));

            // Add entity information
            AuditEvent.AuditEventEntityComponent entity = auditEvent.addEntity();
            entity.setWhat(new Reference("Patient/example"));
            entity.setType(new Coding()
                    .setSystem("http://terminology.hl7.org/CodeSystem/audit-entity-type")
                    .setCode("1")
                    .setDisplay("Person"));

            AuditEvent createdAuditEvent = (AuditEvent) client.create()
                    .resource(auditEvent)
                    .execute()
                    .getResource();

            System.out.println("AuditEvent created with ID: " + createdAuditEvent.getIdElement().getIdPart());
            System.out.println();

            // Step 3: Batch Operations (non-transactional)
            System.out.println("Step 3: Batch Operations (Non-Transactional)...");
            System.out.println("--------------------------------------------");

            Bundle batchBundle = new Bundle();
            batchBundle.setType(Bundle.BundleType.BATCH);

            // Add multiple search operations to the batch
            batchBundle.addEntry()
                    .getRequest()
                        .setMethod(Bundle.HTTPVerb.GET)
                        .setUrl("Patient?family=Smith&_count=1");

            batchBundle.addEntry()
                    .getRequest()
                        .setMethod(Bundle.HTTPVerb.GET)
                        .setUrl("Observation?_count=1");

            System.out.println("Executing batch with " + batchBundle.getEntry().size() + " requests...");

            Bundle batchResponse = client.transaction()
                    .withBundle(batchBundle)
                    .execute();

            System.out.println("Batch completed!");
            System.out.println("Response contains " + batchResponse.getEntry().size() + " entries");

            for (Bundle.BundleEntryComponent entry : batchResponse.getEntry()) {
                if (entry.getResponse() != null) {
                    System.out.println("  - Status: " + entry.getResponse().getStatus());
                }

                if (entry.getResource() instanceof Bundle searchBundle) {
                    System.out.println("    Search returned " + searchBundle.getTotal() + " results");
                }
            }

            System.out.println();

            // Step 4: History Operations
            System.out.println("Step 4: History Operations...");
            System.out.println("--------------------------");

            Bundle patientHistory = client.history()
                    .onType(Patient.class)
                    .returnBundle(Bundle.class)
                    .count(5)
                    .execute();

            System.out.println("Patient history contains " + patientHistory.getEntry().size() + " entries:");

            for (Bundle.BundleEntryComponent entry : patientHistory.getEntry()) {
                if (entry.getResource() instanceof Patient historyPatient) {
                    System.out.println("  - Patient/" + historyPatient.getIdElement().getIdPart() +
                            " (Version: " + historyPatient.getMeta().getVersionId() + ")");
                }
            }

            System.out.println();
            System.out.println("Advanced Operations Best Practices:");
            System.out.println("-----------------------------------");
            System.out.println("1. Use transactions when multiple operations must succeed or fail together.");
            System.out.println("2. Use batches for independent operations that can be executed in parallel.");
            System.out.println("3. Implement proper audit logging for compliance (HIPAA, GDPR, etc.).");
            System.out.println("4. Always use HTTPS and OAuth2 for production systems.");
            System.out.println("5. Monitor and handle transaction failures appropriately.");

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }
}
