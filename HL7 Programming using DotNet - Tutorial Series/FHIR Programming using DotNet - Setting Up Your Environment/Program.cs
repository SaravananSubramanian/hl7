using System;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.SetupEnvironment
{
    /// <summary>
    /// This tutorial demonstrates how to set up your development environment for FHIR programming
    /// using .NET and connect to a FHIR server. This is the first step in working with FHIR
    /// resources programmatically.
    ///
    /// Prerequisites:
    /// - .NET SDK installed (download from https://dotnet.microsoft.com/download)
    /// - A FHIR server to connect to (Azure FHIR Server or HAPI FHIR Server)
    ///
    /// For testing, you can use a public FHIR test server like:
    /// - http://hapi.fhir.org/baseR4
    /// - https://server.fire.ly/r4
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Replace with your FHIR server base URL
            // For testing, you can use a public test server like http://hapi.fhir.org/baseR4
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Environment Setup Tutorial");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            try
            {
                // Create a new FhirClient to connect to the FHIR server
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                Console.WriteLine($"Successfully created FHIR client for server: {fhirServerUrl}");
                Console.WriteLine();

                // Test the connection by requesting the server's capability statement
                Console.WriteLine("Testing connection by retrieving server capability statement...");
                var capabilityStatement = fhirClient.CapabilityStatement();

                if (capabilityStatement != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Connection successful!");
                    Console.WriteLine($"Server Name: {capabilityStatement.Software?.Name ?? "Unknown"}");
                    Console.WriteLine($"Server Version: {capabilityStatement.Software?.Version ?? "Unknown"}");
                    Console.WriteLine($"FHIR Version: {capabilityStatement.FhirVersion}");
                    Console.WriteLine($"Publisher: {capabilityStatement.Publisher ?? "Unknown"}");
                    Console.WriteLine();

                    // Display supported resource types
                    if (capabilityStatement.Rest?.Count > 0)
                    {
                        var rest = capabilityStatement.Rest[0];
                        Console.WriteLine($"Supported Resources ({rest.Resource?.Count ?? 0} types):");

                        int count = 0;
                        foreach (var resource in rest.Resource ?? Enumerable.Empty<Hl7.Fhir.Model.CapabilityStatement.ResourceComponent>())
                        {
                            if (count < 10) // Show first 10 resources
                            {
                                Console.WriteLine($"  - {resource.Type}");
                            }
                            count++;
                        }

                        if (count > 10)
                        {
                            Console.WriteLine($"  ... and {count - 10} more");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to FHIR server: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Make sure the FHIR server URL is correct and the server is accessible.");
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
