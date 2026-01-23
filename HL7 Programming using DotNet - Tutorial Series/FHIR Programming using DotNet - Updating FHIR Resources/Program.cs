using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.UpdateResources
{
    /// <summary>
    /// This tutorial demonstrates how to update FHIR resources using .NET and the FHIR SDK.
    /// Updating resources is essential for maintaining accurate healthcare data.
    /// </summary>
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Updating FHIR Resources Tutorial");
            Console.WriteLine("================================================");
            Console.WriteLine();

            try
            {
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    ReturnPreference = ReturnPreference.Representation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                // Step 1: Create a patient to update
                Console.WriteLine("Step 1: Creating a patient resource to update");
                Console.WriteLine("---------------------------------------------");

                var patient = new Patient
                {
                    Name = new List<HumanName>
                    {
                        new HumanName
                        {
                            Family = "OriginalLastName",
                            Given = new List<string> { "John" }
                        }
                    },
                    Gender = AdministrativeGender.Male,
                    BirthDate = "1980-01-01",
                    Active = true
                };

                var createdPatient = await fhirClient.CreateAsync(patient);
                if (createdPatient?.Name == null || createdPatient.Name.Count == 0)
                {
                    Console.WriteLine("Error: Failed to create patient.");
                    return;
                }

                Console.WriteLine($"Patient created with ID: {createdPatient.Id}");
                Console.WriteLine($"Original Name: {createdPatient.Name[0].Family}, {string.Join(" ", createdPatient.Name[0].Given ?? new List<string>())}");
                Console.WriteLine();

                // Step 2: Update the patient's family name
                Console.WriteLine("Step 2: Updating the patient's family name");
                Console.WriteLine("-------------------------------------------");

                createdPatient.Name[0].Family = "UpdatedLastName";
                var updatedPatient = await fhirClient.UpdateAsync(createdPatient);

                if (updatedPatient?.Name == null || updatedPatient.Name.Count == 0)
                {
                    Console.WriteLine("Error: Failed to update patient.");
                    return;
                }

                Console.WriteLine($"Patient updated successfully!");
                Console.WriteLine($"Updated Name: {updatedPatient.Name[0].Family}, {string.Join(" ", updatedPatient.Name[0].Given ?? new List<string>())}");
                Console.WriteLine();

                // Step 3: Add contact information
                Console.WriteLine("Step 3: Adding contact information");
                Console.WriteLine("-----------------------------------");

                updatedPatient.Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "555-987-6543",
                        Use = ContactPoint.ContactPointUse.Home
                    }
                };

                var patientWithContact = await fhirClient.UpdateAsync(updatedPatient);
                if (patientWithContact?.Telecom?.Count > 0)
                {
                    Console.WriteLine($"Contact added: {patientWithContact.Telecom[0].Value}");
                }
                Console.WriteLine();

                // Step 4: Demonstrate conditional update
                Console.WriteLine("Step 4: Conditional Update (by identifier)");
                Console.WriteLine("------------------------------------------");

                if (patientWithContact == null)
                {
                    Console.WriteLine("Error: Patient with contact is null.");
                    return;
                }

                // Add an identifier to the patient
                patientWithContact.Identifier = new List<Identifier>
                {
                    new Identifier("http://example.org/mrn", "UPDATE-TEST-" + Guid.NewGuid().ToString().Substring(0, 8))
                };

                var patientWithIdentifier = await fhirClient.UpdateAsync(patientWithContact);
                if (patientWithIdentifier?.Identifier?.Count > 0)
                {
                    Console.WriteLine($"Added identifier: {patientWithIdentifier.Identifier[0].Value}");

                    // Now do a conditional update using the identifier
                    if (patientWithIdentifier.Name?.Count > 0)
                    {
                        patientWithIdentifier.Name[0].Family = "ConditionallyUpdatedName";

                        var conditionalResult = await fhirClient.UpdateAsync(
                            patientWithIdentifier,
                            new SearchParams().Where($"identifier={patientWithIdentifier.Identifier[0].System}|{patientWithIdentifier.Identifier[0].Value}"));

                        Console.WriteLine($"Conditional update completed!");
                        if (conditionalResult?.Name?.Count > 0)
                        {
                            Console.WriteLine($"Final Name: {conditionalResult.Name[0].Family}");
                        }
                    }
                }
                Console.WriteLine();

                // Step 5: Handling update conflicts
                Console.WriteLine("Step 5: Handling update conflicts");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Note: In production, always handle version conflicts by reading the latest");
                Console.WriteLine("version and re-applying your changes before attempting another update.");
            }
            catch (FhirOperationException ex)
            {
                Console.WriteLine($"FHIR Operation Error: {ex.Message}");
                if (ex.Status == System.Net.HttpStatusCode.Conflict)
                {
                    Console.WriteLine("A conflict occurred during the update. The resource may have been modified.");
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
