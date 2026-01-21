using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.ReadResources
{
    /// <summary>
    /// This tutorial demonstrates how to read FHIR resources using .NET and the FHIR SDK.
    /// Reading resources is a fundamental operation that allows you to retrieve healthcare data
    /// from a FHIR server.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Reading FHIR Resources Tutorial");
            Console.WriteLine("===============================================");
            Console.WriteLine();

            try
            {
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                // Step 1: Read a Patient resource by ID
                Console.WriteLine("Step 1: Reading a Patient resource by ID");
                Console.WriteLine("-----------------------------------------");

                // First, search for any patient to get a valid ID
                var searchResult = fhirClient.Search<Patient>(new SearchParams().LimitTo(1));

                if (searchResult.Entry?.Count > 0)
                {
                    var firstPatient = (Patient)searchResult.Entry[0].Resource;
                    string patientId = firstPatient.Id;

                    Console.WriteLine($"Found patient ID: {patientId}");
                    Console.WriteLine();

                    // Read the specific patient by ID
                    var patient = fhirClient.Read<Patient>($"Patient/{patientId}");

                    Console.WriteLine("Patient Details:");
                    Console.WriteLine($"  ID: {patient.Id}");

                    if (patient.Name?.Count > 0)
                    {
                        var name = patient.Name[0];
                        Console.WriteLine($"  Name: {name.Family}, {string.Join(" ", name.Given ?? new List<string>())}");
                    }

                    Console.WriteLine($"  Gender: {patient.Gender}");
                    Console.WriteLine($"  Birth Date: {patient.BirthDate}");
                    Console.WriteLine($"  Active: {patient.Active}");
                }
                else
                {
                    Console.WriteLine("No patients found on the server.");
                }

                // Step 2: Search for Patients by criteria
                Console.WriteLine();
                Console.WriteLine("Step 2: Searching for Patients by family name");
                Console.WriteLine("---------------------------------------------");

                var searchParams = new SearchParams()
                    .Where("family=Smith")
                    .LimitTo(5);

                var searchBundle = fhirClient.Search<Patient>(searchParams);

                Console.WriteLine($"Found {searchBundle.Total} patient(s) with family name 'Smith'");
                Console.WriteLine();

                foreach (var entry in searchBundle.Entry ?? new List<Bundle.EntryComponent>())
                {
                    var patient = (Patient)entry.Resource;
                    if (patient.Name?.Count > 0)
                    {
                        Console.WriteLine($"  - {patient.Name[0].Family}, {string.Join(" ", patient.Name[0].Given ?? new List<string>())} (ID: {patient.Id})");
                    }
                    else
                    {
                        Console.WriteLine($"  - (No name) ID: {patient.Id}");
                    }
                }

                // Step 3: Error handling for non-existent resources
                Console.WriteLine();
                Console.WriteLine("Step 3: Handling errors when reading non-existent resources");
                Console.WriteLine("-----------------------------------------------------------");

                try
                {
                    var nonExistentPatient = fhirClient.Read<Patient>("Patient/non-existent-id-12345");
                }
                catch (FhirOperationException ex)
                {
                    Console.WriteLine($"Expected error caught: {ex.Message}");
                    if (ex.Status == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("  The specified patient could not be found (404 Not Found).");
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
