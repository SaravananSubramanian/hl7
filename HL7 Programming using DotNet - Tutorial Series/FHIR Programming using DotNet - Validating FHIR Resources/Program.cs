using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace Com.SaravananSubramanian.Fhir.ValidateResources
{
    /// <summary>
    /// This tutorial demonstrates how to validate FHIR resources using .NET and the FHIR SDK.
    /// Validation ensures that resources conform to FHIR specifications and custom profiles.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FHIR .NET SDK - Validating FHIR Resources Tutorial");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            try
            {
                // Step 1: Basic serialization validation of a well-formed Patient resource
                Console.WriteLine("Step 1: Validating a Well-Formed Patient Resource");
                Console.WriteLine("-------------------------------------------------");

                var validPatient = new Patient
                {
                    Name = new List<HumanName>
                    {
                        new HumanName
                        {
                            Family = "Doe",
                            Given = new List<string> { "John" }
                        }
                    },
                    Gender = AdministrativeGender.Male,
                    BirthDate = "1980-01-01"
                };

                // Use the JSON serializer to verify the resource is well-formed
                var serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = true });

                try
                {
                    string json = serializer.SerializeToString(validPatient);
                    Console.WriteLine("Resource serializes correctly (well-formed).");
                    Console.WriteLine("Serialized length: " + json.Length + " characters");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Serialization error: {ex.Message}");
                }

                Console.WriteLine();

                // Step 2: Validate an Observation resource
                Console.WriteLine("Step 2: Validating an Observation Resource");
                Console.WriteLine("------------------------------------------");

                var observation = new Observation
                {
                    Status = ObservationStatus.Final,
                    Code = new CodeableConcept("http://loinc.org", "8867-4", "Heart rate"),
                    Subject = new ResourceReference("Patient/123"),
                    Value = new Quantity
                    {
                        Value = 72,
                        Unit = "beats/minute",
                        System = "http://unitsofmeasure.org",
                        Code = "/min"
                    }
                };

                try
                {
                    string json = serializer.SerializeToString(observation);
                    Console.WriteLine("Observation resource is well-formed.");
                    Console.WriteLine("Serialized length: " + json.Length + " characters");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Serialization error: {ex.Message}");
                }

                Console.WriteLine();

                // Step 3: Server-side validation using $validate operation
                Console.WriteLine("Step 3: Server-Side Validation using $validate");
                Console.WriteLine("----------------------------------------------");

                string fhirServerUrl = "http://hapi.fhir.org/baseR4";
                var fhirClient = new FhirClient(fhirServerUrl);

                // Use the $validate operation on the server
                var parameters = new Parameters();
                parameters.Add("resource", validPatient);

                try
                {
                    var serverValidationResult = fhirClient.TypeOperation<Patient>("validate", parameters);

                    if (serverValidationResult is OperationOutcome serverOutcome)
                    {
                        Console.WriteLine("Server validation completed.");
                        PrintValidationOutcome(serverOutcome);
                    }
                    else if (serverValidationResult is Parameters resultParams)
                    {
                        Console.WriteLine("Server validation completed.");
                        foreach (var param in resultParams.Parameter)
                        {
                            if (param.Resource is OperationOutcome outcome)
                            {
                                PrintValidationOutcome(outcome);
                            }
                        }
                    }
                }
                catch (FhirOperationException ex)
                {
                    if (ex.Outcome != null)
                    {
                        Console.WriteLine("Server validation returned issues:");
                        PrintValidationOutcome(ex.Outcome);
                    }
                    else
                    {
                        Console.WriteLine($"Server validation error: {ex.Message}");
                    }
                }

                Console.WriteLine();

                // Step 4: Validate Observation on the server
                Console.WriteLine("Step 4: Server-Side Validation of Observation");
                Console.WriteLine("---------------------------------------------");

                var obsParameters = new Parameters();
                obsParameters.Add("resource", observation);

                try
                {
                    var obsValidationResult = fhirClient.TypeOperation<Observation>("validate", obsParameters);

                    if (obsValidationResult is OperationOutcome obsOutcome)
                    {
                        Console.WriteLine("Observation server validation completed.");
                        PrintValidationOutcome(obsOutcome);
                    }
                    else if (obsValidationResult is Parameters resultParams)
                    {
                        Console.WriteLine("Observation server validation completed.");
                        foreach (var param in resultParams.Parameter)
                        {
                            if (param.Resource is OperationOutcome outcome)
                            {
                                PrintValidationOutcome(outcome);
                            }
                        }
                    }
                }
                catch (FhirOperationException ex)
                {
                    if (ex.Outcome != null)
                    {
                        Console.WriteLine("Server validation returned issues:");
                        PrintValidationOutcome(ex.Outcome);
                    }
                    else
                    {
                        Console.WriteLine($"Server validation error: {ex.Message}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Validation Best Practices:");
                Console.WriteLine("--------------------------");
                Console.WriteLine("1. Always validate resources before sending to production servers.");
                Console.WriteLine("2. Use profiles to enforce business rules beyond base FHIR specs.");
                Console.WriteLine("3. Combine client-side and server-side validation for comprehensive checks.");
                Console.WriteLine("4. Log validation issues for audit and debugging purposes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            if (Environment.GetEnvironmentVariable("FHIR_TUTORIAL_TEST_MODE") != "true")
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void PrintValidationOutcome(OperationOutcome outcome)
        {
            foreach (var issue in outcome.Issue)
            {
                string severity = issue.Severity?.ToString() ?? "Unknown";
                string details = issue.Diagnostics ?? issue.Details?.Text ?? "No details";
                string location = issue.Expression != null && issue.Expression.Any()
                    ? string.Join(", ", issue.Expression)
                    : "N/A";

                Console.WriteLine($"  [{severity}] {details}");
                if (location != "N/A")
                {
                    Console.WriteLine($"    Location: {location}");
                }
            }

            if (outcome.Issue.Count == 0)
            {
                Console.WriteLine("  No issues found - validation passed.");
            }
        }
    }
}
