using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.ChainingOperations
{
    /// <summary>
    /// This tutorial demonstrates how to chain FHIR operations using .NET and the FHIR SDK.
    /// Chaining allows you to perform complex queries that span across multiple related resources.
    /// </summary>
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Chaining FHIR Operations Tutorial");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            try
            {
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    ReturnPreference = ReturnPreference.Representation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                // Step 1: Basic chained query - Find Observations for a Patient
                Console.WriteLine("Step 1: Basic Chained Query - Find Observations for a Patient");
                Console.WriteLine("-------------------------------------------------------------");

                // First, find a patient to work with
                var patientSearch = await fhirClient.SearchAsync<Patient>(new SearchParams().LimitTo(1));

                if (patientSearch?.Entry?.Count > 0 && patientSearch.Entry[0].Resource is Patient patient)
                {
                    string patientId = patient.Id ?? "unknown";

                    Console.WriteLine($"Using Patient ID: {patientId}");
                    Console.WriteLine();

                    // Search for Observations linked to this Patient
                    var observations = await fhirClient.SearchAsync<Observation>(new SearchParams()
                        .Where($"patient={patientId}")
                        .LimitTo(5));

                    Console.WriteLine($"Found {observations?.Total ?? 0} Observations for this patient:");

                    foreach (var entry in observations?.Entry ?? new List<Bundle.EntryComponent>())
                    {
                        if (entry.Resource is Observation obs)
                        {
                            Console.WriteLine($"  - Observation ID: {obs.Id}, Status: {obs.Status}");

                            if (obs.Code?.Coding?.Count > 0)
                            {
                                var coding = obs.Code.Coding[0];
                                Console.WriteLine($"    Code: {coding.Display ?? coding.Code}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No patients found on the server.");
                }

                Console.WriteLine();

                // Step 2: Chained search with multiple parameters
                Console.WriteLine("Step 2: Chained Search - Find Encounters for Patients with Conditions");
                Console.WriteLine("---------------------------------------------------------------------");

                // This searches for encounters based on patient criteria
                var encounterSearch = await fhirClient.SearchAsync<Encounter>(new SearchParams()
                    .Where("patient:Patient.family=Smith")
                    .LimitTo(5));

                Console.WriteLine($"Found {encounterSearch?.Total ?? 0} Encounters for patients named Smith:");

                foreach (var entry in encounterSearch?.Entry ?? new List<Bundle.EntryComponent>())
                {
                    if (entry.Resource is Encounter encounter)
                    {
                        Console.WriteLine($"  - Encounter ID: {encounter.Id}, Status: {encounter.Status}");
                    }
                }

                Console.WriteLine();

                // Step 3: Handling paginated chained results
                Console.WriteLine("Step 3: Handling Paginated Chained Results");
                Console.WriteLine("------------------------------------------");

                var resultSet = await fhirClient.SearchAsync<Observation>(new SearchParams()
                    .LimitTo(3));

                int pageNum = 1;
                int totalProcessed = 0;
                int maxPages = 3; // Limit for demo purposes

                while (resultSet != null && pageNum <= maxPages)
                {
                    Console.WriteLine($"Page {pageNum}: {resultSet.Entry?.Count ?? 0} results");

                    foreach (var entry in resultSet.Entry ?? new List<Bundle.EntryComponent>())
                    {
                        if (entry.Resource is Observation obs)
                        {
                            Console.WriteLine($"  - {obs.Id}: {obs.Status}");
                            totalProcessed++;
                        }
                    }

                    // Get the next page
                    if (resultSet.NextLink != null && pageNum < maxPages)
                    {
                        resultSet = await fhirClient.ContinueAsync(resultSet);
                        pageNum++;
                    }
                    else
                    {
                        break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"Total observations processed: {totalProcessed}");
                Console.WriteLine();

                // Step 4: Using _include to get related resources
                Console.WriteLine("Step 4: Using _include to Get Related Resources");
                Console.WriteLine("-----------------------------------------------");

                var includeSearch = await fhirClient.SearchAsync<Observation>(new SearchParams()
                    .Include("Observation:patient")
                    .LimitTo(3));

                Console.WriteLine($"Found {includeSearch?.Entry?.Count ?? 0} resources (including related):");

                foreach (var entry in includeSearch?.Entry ?? new List<Bundle.EntryComponent>())
                {
                    var resource = entry.Resource;
                    if (resource != null)
                    {
                        Console.WriteLine($"  - {resource.TypeName}/{resource.Id}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Chaining Best Practices:");
                Console.WriteLine("------------------------");
                Console.WriteLine("1. Chained queries reduce the number of API calls needed.");
                Console.WriteLine("2. Use _include and _revinclude to fetch related resources efficiently.");
                Console.WriteLine("3. Always handle pagination for large result sets.");
                Console.WriteLine("4. Be mindful of server-side performance with complex chains.");
            }
            catch (FhirOperationException ex)
            {
                Console.WriteLine($"FHIR Operation Error: {ex.Message}");
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
