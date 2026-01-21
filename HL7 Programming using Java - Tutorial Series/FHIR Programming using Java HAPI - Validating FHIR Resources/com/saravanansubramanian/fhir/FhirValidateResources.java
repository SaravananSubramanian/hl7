package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.context.support.DefaultProfileValidationSupport;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import ca.uhn.fhir.validation.FhirValidator;
import ca.uhn.fhir.validation.ValidationResult;
import org.hl7.fhir.common.hapi.validation.support.*;
import org.hl7.fhir.common.hapi.validation.validator.FhirInstanceValidator;
import org.hl7.fhir.r4.model.*;

/**
 * This tutorial demonstrates how to validate FHIR resources using Java and the HAPI FHIR library.
 * Validation ensures that resources conform to FHIR specifications and custom profiles.
 */
public class FhirValidateResources {

    public static void main(String[] args) {
        System.out.println("FHIR Java SDK - Validating FHIR Resources Tutorial");
        System.out.println("==================================================");
        System.out.println();

        try {
            // Initialize FHIR context
            FhirContext ctx = FhirContext.forR4();

            // Step 1: Set up the validator
            System.out.println("Step 1: Setting up the FHIR Validator...");
            System.out.println("---------------------------------------");

            // Create a validation support chain
            ValidationSupportChain validationSupportChain = new ValidationSupportChain(
                    new DefaultProfileValidationSupport(ctx),
                    new InMemoryTerminologyServerValidationSupport(ctx),
                    new CommonCodeSystemsTerminologyService(ctx)
            );

            // Create a FHIR validator
            FhirValidator validator = ctx.newValidator();

            // Create an instance validator and configure it
            FhirInstanceValidator instanceValidator = new FhirInstanceValidator(validationSupportChain);
            validator.registerValidatorModule(instanceValidator);

            System.out.println("Validator configured successfully.");
            System.out.println();

            // Step 2: Validate a well-formed Patient resource
            System.out.println("Step 2: Validating a Well-Formed Patient Resource...");
            System.out.println("---------------------------------------------------");

            Patient validPatient = new Patient();
            validPatient.addName()
                    .setFamily("Doe")
                    .addGiven("John");
            validPatient.setGender(Enumerations.AdministrativeGender.MALE);
            validPatient.setBirthDateElement(new DateType("1980-01-01"));

            ValidationResult validResult = validator.validateWithResult(validPatient);

            System.out.println("Validation Result: " + (validResult.isSuccessful() ? "PASSED" : "FAILED"));
            printValidationResult(validResult);
            System.out.println();

            // Step 3: Validate an Observation resource
            System.out.println("Step 3: Validating an Observation Resource...");
            System.out.println("--------------------------------------------");

            Observation observation = new Observation();
            observation.setStatus(Observation.ObservationStatus.FINAL);
            observation.setCode(new CodeableConcept()
                    .addCoding(new Coding()
                            .setSystem("http://loinc.org")
                            .setCode("8867-4")
                            .setDisplay("Heart rate")));
            observation.setSubject(new Reference("Patient/123"));
            observation.setValue(new Quantity()
                    .setValue(72)
                    .setUnit("beats/minute")
                    .setSystem("http://unitsofmeasure.org")
                    .setCode("/min"));

            ValidationResult obsResult = validator.validateWithResult(observation);

            System.out.println("Validation Result: " + (obsResult.isSuccessful() ? "PASSED" : "FAILED"));
            printValidationResult(obsResult);
            System.out.println();

            // Step 4: Server-side validation
            System.out.println("Step 4: Server-Side Validation...");
            System.out.println("-------------------------------");

            String fhirServerUrl = "http://hapi.fhir.org/baseR4";
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Create a Parameters resource for the $validate operation
            Parameters parameters = new Parameters();
            parameters.addParameter().setName("resource").setResource(validPatient);

            try {
                Parameters serverValidationResult = client
                        .operation()
                        .onType(Patient.class)
                        .named("$validate")
                        .withParameters(parameters)
                        .execute();

                System.out.println("Server validation completed.");

                // Extract the OperationOutcome from the response
                for (Parameters.ParametersParameterComponent param : serverValidationResult.getParameter()) {
                    if (param.hasResource() && param.getResource() instanceof OperationOutcome) {
                        OperationOutcome outcome = (OperationOutcome) param.getResource();
                        for (OperationOutcome.OperationOutcomeIssueComponent issue : outcome.getIssue()) {
                            System.out.println("  [" + issue.getSeverity() + "] " + issue.getDiagnostics());
                        }
                    }
                }
            } catch (Exception e) {
                System.out.println("Server validation error: " + e.getMessage());
            }

            System.out.println();
            System.out.println("Validation Best Practices:");
            System.out.println("--------------------------");
            System.out.println("1. Always validate resources before sending to production servers.");
            System.out.println("2. Use profiles to enforce business rules beyond base FHIR specs.");
            System.out.println("3. Combine client-side and server-side validation for comprehensive checks.");
            System.out.println("4. Log validation issues for audit and debugging purposes.");

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }

    private static void printValidationResult(ValidationResult result) {
        if (result.getMessages().isEmpty()) {
            System.out.println("  No issues found.");
        } else {
            for (var message : result.getMessages()) {
                System.out.println("  [" + message.getSeverity() + "] " + message.getMessage());
                if (message.getLocationString() != null) {
                    System.out.println("    Location: " + message.getLocationString());
                }
            }
        }
    }
}
