using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.CreateResources
{
    /// <summary>
    /// This tutorial demonstrates how to create FHIR resources using .NET and the FHIR SDK.
    /// We'll focus on creating a Patient resource, which is one of the most commonly used
    /// resources in FHIR.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Creating FHIR Resources Tutorial");
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

                // Create a new Patient resource
                var patient = new Patient
                {
                    Identifier = new List<Identifier>
                    {
                        new Identifier("http://hospital.smarthealthit.org", "123456")
                    },
                    Name = new List<HumanName>
                    {
                        new HumanName
                        {
                            Use = HumanName.NameUse.Official,
                            Family = "Doe",
                            Given = new List<string> { "John", "Adam" },
                            Prefix = new List<string> { "Mr." }
                        }
                    },
                    Gender = AdministrativeGender.Male,
                    BirthDate = "1980-01-01",
                    Active = true,
                    Address = new List<Address>
                    {
                        new Address
                        {
                            Use = Address.AddressUse.Home,
                            Type = Address.AddressType.Physical,
                            Line = new List<string> { "123 Main St" },
                            City = "Metropolis",
                            State = "NY",
                            PostalCode = "10001",
                            Country = "USA"
                        }
                    },
                    Telecom = new List<ContactPoint>
                    {
                        new ContactPoint
                        {
                            System = ContactPoint.ContactPointSystem.Phone,
                            Value = "555-123-4567",
                            Use = ContactPoint.ContactPointUse.Home
                        },
                        new ContactPoint
                        {
                            System = ContactPoint.ContactPointSystem.Email,
                            Value = "john.doe@example.com",
                            Use = ContactPoint.ContactPointUse.Work
                        }
                    }
                };

                Console.WriteLine("Patient resource created locally:");
                Console.WriteLine($"  Name: {patient.Name[0].Family}, {string.Join(" ", patient.Name[0].Given)}");
                Console.WriteLine($"  Gender: {patient.Gender}");
                Console.WriteLine($"  Birth Date: {patient.BirthDate}");
                Console.WriteLine();

                // Send the Patient to the FHIR server
                Console.WriteLine("Sending Patient resource to FHIR server...");
                var createdPatient = fhirClient.Create(patient);

                Console.WriteLine();
                Console.WriteLine("Patient created successfully on the FHIR server!");
                Console.WriteLine($"  Resource ID: {createdPatient.Id}");
                Console.WriteLine($"  Resource URL: {fhirServerUrl}/Patient/{createdPatient.Id}");

                // Verify the creation by reading it back
                Console.WriteLine();
                Console.WriteLine("Verifying creation by reading the patient back...");
                var retrievedPatient = fhirClient.Read<Patient>($"Patient/{createdPatient.Id}");

                Console.WriteLine();
                Console.WriteLine("Retrieved Patient:");
                Console.WriteLine($"  ID: {retrievedPatient.Id}");
                Console.WriteLine($"  Name: {retrievedPatient.Name[0].Family}, {string.Join(" ", retrievedPatient.Name[0].Given)}");
                Console.WriteLine($"  Gender: {retrievedPatient.Gender}");
                Console.WriteLine($"  Active: {retrievedPatient.Active}");
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
