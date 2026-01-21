using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.AdvancedOperations
{
    /// <summary>
    /// This tutorial demonstrates advanced FHIR operations using .NET and the FHIR SDK.
    /// Topics covered include security, auditing, and transaction management.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Replace with your FHIR server base URL
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Advanced Operations Tutorial");
            Console.WriteLine("============================================");
            Console.WriteLine();

            try
            {
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                // Step 1: Creating FHIR Transactions
                Console.WriteLine("Step 1: FHIR Transactions (Bundle Operations)");
                Console.WriteLine("---------------------------------------------");

                // Create a transaction bundle
                var transactionBundle = new Bundle
                {
                    Type = Bundle.BundleType.Transaction
                };

                // Add a Patient to the transaction
                var patient = new Patient
                {
                    Name = new List<HumanName>
                    {
                        new HumanName { Family = "TransactionTest", Given = new List<string> { "John" } }
                    },
                    Gender = AdministrativeGender.Male
                };

                // Generate a temporary UUID for the patient
                string patientUuid = $"urn:uuid:{Guid.NewGuid()}";

                transactionBundle.Entry.Add(new Bundle.EntryComponent
                {
                    FullUrl = patientUuid,
                    Resource = patient,
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.POST,
                        Url = "Patient"
                    }
                });

                // Add an Observation linked to the Patient
                var observation = new Observation
                {
                    Status = ObservationStatus.Final,
                    Code = new CodeableConcept("http://loinc.org", "8867-4", "Heart rate"),
                    Subject = new ResourceReference(patientUuid), // Reference the patient by UUID
                    Value = new Quantity
                    {
                        Value = 72,
                        Unit = "beats/minute",
                        System = "http://unitsofmeasure.org",
                        Code = "/min"
                    }
                };

                transactionBundle.Entry.Add(new Bundle.EntryComponent
                {
                    Resource = observation,
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.POST,
                        Url = "Observation"
                    }
                });

                Console.WriteLine($"Executing transaction with {transactionBundle.Entry.Count} entries...");

                var transactionResponse = fhirClient.Transaction(transactionBundle);

                Console.WriteLine("Transaction completed successfully!");
                Console.WriteLine($"Response contains {transactionResponse.Entry.Count} entries:");

                foreach (var entry in transactionResponse.Entry)
                {
                    if (entry.Response != null)
                    {
                        Console.WriteLine($"  - {entry.Response.Location} (Status: {entry.Response.Status})");
                    }
                }

                Console.WriteLine();

                // Step 2: Creating Audit Events
                Console.WriteLine("Step 2: Creating Audit Events");
                Console.WriteLine("-----------------------------");

                var auditEvent = new AuditEvent
                {
                    Type = new Coding("http://terminology.hl7.org/CodeSystem/audit-event-type", "rest", "RESTful Operation"),
                    Action = AuditEvent.AuditEventAction.C,
                    Outcome = AuditEvent.AuditEventOutcome.N0,
                    Recorded = DateTimeOffset.Now,
                    Agent = new List<AuditEvent.AgentComponent>
                    {
                        new AuditEvent.AgentComponent
                        {
                            Who = new ResourceReference("Practitioner/example"),
                            Requestor = true
                        }
                    },
                    Source = new AuditEvent.SourceComponent
                    {
                        Observer = new ResourceReference("Device/software-system")
                    },
                    Entity = new List<AuditEvent.EntityComponent>
                    {
                        new AuditEvent.EntityComponent
                        {
                            What = new ResourceReference("Patient/example"),
                            Type = new Coding("http://terminology.hl7.org/CodeSystem/audit-entity-type", "1", "Person")
                        }
                    }
                };

                var createdAuditEvent = fhirClient.Create(auditEvent);
                Console.WriteLine($"AuditEvent created with ID: {createdAuditEvent.Id}");
                Console.WriteLine();

                // Step 3: Batch Operations (non-transactional)
                Console.WriteLine("Step 3: Batch Operations (Non-Transactional)");
                Console.WriteLine("--------------------------------------------");

                var batchBundle = new Bundle
                {
                    Type = Bundle.BundleType.Batch
                };

                // Add multiple search operations to the batch
                batchBundle.Entry.Add(new Bundle.EntryComponent
                {
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.GET,
                        Url = "Patient?family=Smith&_count=1"
                    }
                });

                batchBundle.Entry.Add(new Bundle.EntryComponent
                {
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.GET,
                        Url = "Observation?_count=1"
                    }
                });

                Console.WriteLine($"Executing batch with {batchBundle.Entry.Count} requests...");

                var batchResponse = fhirClient.Transaction(batchBundle);

                Console.WriteLine("Batch completed!");
                Console.WriteLine($"Response contains {batchResponse.Entry.Count} entries");

                foreach (var entry in batchResponse.Entry)
                {
                    if (entry.Response != null)
                    {
                        Console.WriteLine($"  - Status: {entry.Response.Status}");
                    }

                    if (entry.Resource is Bundle searchBundle)
                    {
                        Console.WriteLine($"    Search returned {searchBundle.Total} results");
                    }
                }

                Console.WriteLine();

                // Step 4: History Operations
                Console.WriteLine("Step 4: History Operations");
                Console.WriteLine("--------------------------");

                // Get history for a specific resource type using TypeHistory
                var patientHistory = fhirClient.TypeHistory<Patient>(pageSize: 5);

                Console.WriteLine($"Patient history contains {patientHistory.Entry?.Count ?? 0} entries:");

                foreach (var entry in patientHistory.Entry ?? new List<Bundle.EntryComponent>())
                {
                    var resource = entry.Resource as Patient;
                    if (resource != null)
                    {
                        Console.WriteLine($"  - Patient/{resource.Id} (Version: {resource.Meta?.VersionId})");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Advanced Operations Best Practices:");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("1. Use transactions when multiple operations must succeed or fail together.");
                Console.WriteLine("2. Use batches for independent operations that can be executed in parallel.");
                Console.WriteLine("3. Implement proper audit logging for compliance (HIPAA, GDPR, etc.).");
                Console.WriteLine("4. Always use HTTPS and OAuth2 for production systems.");
                Console.WriteLine("5. Monitor and handle transaction failures appropriately.");
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
