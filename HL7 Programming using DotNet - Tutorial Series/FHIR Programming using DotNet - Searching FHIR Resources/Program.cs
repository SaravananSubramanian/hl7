using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.SearchResources
{
    /// <summary>
    /// This tutorial demonstrates how to search for FHIR resources using .NET and the FHIR SDK.
    /// Searching is a fundamental operation for retrieving specific data from a FHIR server.
    /// </summary>
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Searching FHIR Resources Tutorial");
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

                // Step 1: Basic search by criteria
                Console.WriteLine("Step 1: Basic Search by Family Name");
                Console.WriteLine("------------------------------------");

                var searchResult = await fhirClient.SearchAsync<Patient>(new SearchParams()
                    .Where("family=Smith")
                    .LimitTo(5));

                Console.WriteLine($"Found {searchResult?.Total ?? 0} patient(s) with family name 'Smith'");
                if (searchResult != null)
                {
                    PrintPatients(searchResult);
                }
                Console.WriteLine();

                // Step 2: Advanced search with multiple criteria
                Console.WriteLine("Step 2: Advanced Search with Multiple Criteria");
                Console.WriteLine("-----------------------------------------------");

                var advancedSearchResult = await fhirClient.SearchAsync<Patient>(new SearchParams()
                    .Where("family=Smith")
                    .Where("gender=male")
                    .LimitTo(5));

                Console.WriteLine($"Found {advancedSearchResult?.Total ?? 0} male patient(s) named 'Smith'");
                if (advancedSearchResult != null)
                {
                    PrintPatients(advancedSearchResult);
                }
                Console.WriteLine();

                // Step 3: Search with date range
                Console.WriteLine("Step 3: Search with Date Range");
                Console.WriteLine("-------------------------------");

                var dateSearchResult = await fhirClient.SearchAsync<Patient>(new SearchParams()
                    .Where("birthdate=ge1980-01-01")
                    .Where("birthdate=le1990-12-31")
                    .LimitTo(5));

                Console.WriteLine($"Found {dateSearchResult?.Total ?? 0} patient(s) born between 1980 and 1990");
                if (dateSearchResult != null)
                {
                    PrintPatients(dateSearchResult);
                }
                Console.WriteLine();

                // Step 4: Paginated search results
                Console.WriteLine("Step 4: Handling Paginated Search Results");
                Console.WriteLine("------------------------------------------");

                var paginatedResult = await fhirClient.SearchAsync<Patient>(new SearchParams()
                    .LimitTo(3)); // Limit to 3 per page

                Console.WriteLine($"Page 1: Showing {paginatedResult?.Entry?.Count ?? 0} of {paginatedResult?.Total ?? 0} total patients");
                if (paginatedResult != null)
                {
                    PrintPatients(paginatedResult);

                    // Check if there's a next page
                    if (paginatedResult.NextLink != null)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Fetching next page...");
                        var nextPage = await fhirClient.ContinueAsync(paginatedResult);
                        if (nextPage != null)
                        {
                            Console.WriteLine($"Page 2: Showing {nextPage.Entry?.Count ?? 0} patients");
                            PrintPatients(nextPage);
                        }
                    }
                }

                Console.WriteLine();

                // Step 5: Search for Observations
                Console.WriteLine("Step 5: Searching for Observations");
                Console.WriteLine("-----------------------------------");

                var obsSearchResult = await fhirClient.SearchAsync<Observation>(new SearchParams()
                    .Where("code=http://loinc.org|8867-4") // Heart rate
                    .LimitTo(3));

                Console.WriteLine($"Found {obsSearchResult?.Total ?? 0} heart rate observations");

                foreach (var entry in obsSearchResult?.Entry ?? new List<Bundle.EntryComponent>())
                {
                    if (entry.Resource is Observation obs)
                    {
                        Console.WriteLine($"  - ID: {obs.Id}, Status: {obs.Status}");

                        if (obs.Value is Quantity qty)
                        {
                            Console.WriteLine($"    Value: {qty.Value} {qty.Unit}");
                        }
                    }
                }
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

        static void PrintPatients(Bundle searchResult)
        {
            foreach (var entry in searchResult.Entry ?? new List<Bundle.EntryComponent>())
            {
                if (entry.Resource is Patient patient)
                {
                    string name = "Unknown";

                    if (patient.Name?.Count > 0)
                    {
                        var humanName = patient.Name[0];
                        name = $"{humanName.Family}, {string.Join(" ", humanName.Given ?? new List<string>())}";
                    }

                    Console.WriteLine($"  - {name} (ID: {patient.Id}, Gender: {patient.Gender}, DOB: {patient.BirthDate ?? "N/A"})");
                }
            }
        }
    }
}
