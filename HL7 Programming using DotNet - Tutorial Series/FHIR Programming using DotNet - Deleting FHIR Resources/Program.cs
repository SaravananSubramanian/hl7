using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.DeleteResources
{
    /// <summary>
    /// This tutorial demonstrates how to delete FHIR resources using .NET and the FHIR SDK.
    /// Deleting resources is an important part of managing healthcare data lifecycle.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Deleting FHIR Resources Tutorial");
            Console.WriteLine("================================================");
            Console.WriteLine();

            try
            {
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                // Step 1: Create a patient to delete
                Console.WriteLine("Step 1: Creating a patient resource to delete");
                Console.WriteLine("---------------------------------------------");

                var patient = new Patient
                {
                    Name = new List<HumanName>
                    {
                        new HumanName
                        {
                            Family = "ToBeDeleted",
                            Given = new List<string> { "Test" }
                        }
                    },
                    Gender = AdministrativeGender.Male,
                    Active = true
                };

                var createdPatient = fhirClient.Create(patient);
                string patientId = createdPatient.Id;
                Console.WriteLine($"Patient created with ID: {patientId}");
                Console.WriteLine();

                // Step 2: Delete the patient by ID
                Console.WriteLine("Step 2: Deleting the Patient resource by ID");
                Console.WriteLine("-------------------------------------------");

                fhirClient.Delete($"Patient/{patientId}");
                Console.WriteLine($"Patient/{patientId} deleted successfully.");
                Console.WriteLine();

                // Step 3: Verify deletion by attempting to read
                Console.WriteLine("Step 3: Verifying deletion by attempting to read the patient");
                Console.WriteLine("------------------------------------------------------------");

                try
                {
                    var deletedPatient = fhirClient.Read<Patient>($"Patient/{patientId}");
                    Console.WriteLine("Warning: Resource still exists after deletion!");
                }
                catch (FhirOperationException ex)
                {
                    if (ex.Status == System.Net.HttpStatusCode.Gone || ex.Status == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("Verified: The patient resource has been deleted (404/410 response).");
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                    }
                }

                Console.WriteLine();

                // Step 4: Conditional delete
                Console.WriteLine("Step 4: Demonstrating Conditional Delete");
                Console.WriteLine("-----------------------------------------");

                // Create a patient with a unique identifier
                string uniqueIdentifier = "DELETE-TEST-" + Guid.NewGuid().ToString().Substring(0, 8);
                var patientForConditionalDelete = new Patient
                {
                    Identifier = new List<Identifier>
                    {
                        new Identifier("http://example.org/mrn", uniqueIdentifier)
                    },
                    Name = new List<HumanName>
                    {
                        new HumanName
                        {
                            Family = "ConditionalDelete",
                            Given = new List<string> { "Test" }
                        }
                    }
                };

                var createdForDelete = fhirClient.Create(patientForConditionalDelete);
                Console.WriteLine($"Created patient with identifier: {uniqueIdentifier}");

                // Perform conditional delete based on identifier
                fhirClient.Delete($"Patient?identifier=http://example.org/mrn|{uniqueIdentifier}");
                Console.WriteLine($"Conditional delete completed for identifier: {uniqueIdentifier}");
                Console.WriteLine();

                Console.WriteLine("Important Notes about FHIR Deletions:");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("1. Some FHIR servers may not support deletion (check CapabilityStatement).");
                Console.WriteLine("2. Deleted resources may still be retrievable via history.");
                Console.WriteLine("3. Conditional deletes should be used carefully to avoid unintended data loss.");
                Console.WriteLine("4. Consider using logical deletion (setting active=false) instead of physical deletion.");
            }
            catch (FhirOperationException ex)
            {
                Console.WriteLine($"FHIR Operation Error: {ex.Message}");
                if (ex.Outcome != null)
                {
                    foreach (var issue in ex.Outcome.Issue)
                    {
                        Console.WriteLine($"  Issue: {issue.Diagnostics}");
                    }
                }
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
    }
}
