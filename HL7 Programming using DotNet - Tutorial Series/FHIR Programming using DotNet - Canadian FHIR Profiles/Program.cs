using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace Com.SaravananSubramanian.Fhir.CanadianProfiles
{
    /// <summary>
    /// This tutorial demonstrates how to work with Canadian national FHIR profiles
    /// using .NET and the Firely FHIR SDK.
    ///
    /// Canadian FHIR Profiles Overview:
    /// ================================
    /// Canada Health Infoway has developed the CA Core (Canadian Core) FHIR profiles
    /// that provide Canadian-specific constraints and extensions on base FHIR resources.
    /// These profiles ensure interoperability across Canadian healthcare systems.
    ///
    /// Key Canadian FHIR Profile Components:
    /// -------------------------------------
    /// 1. Provincial Health Numbers (PHN) - Unique identifiers issued by each province
    /// 2. Canadian Postal Codes - Format validation (e.g., A1A 1A1)
    /// 3. Province/Territory codes - Canadian jurisdictional codes
    /// 4. Canadian address extensions - Specific to Canadian addressing standards
    ///
    /// Profile URLs referenced in this tutorial:
    /// - CA Core Patient: http://hl7.org/fhir/ca/core/StructureDefinition/profile-patient
    /// - CA Core Practitioner: http://hl7.org/fhir/ca/core/StructureDefinition/profile-practitioner
    /// - CA Core Organization: http://hl7.org/fhir/ca/core/StructureDefinition/profile-organization
    /// </summary>
    class Program
    {
        // Canadian identifier system URLs
        private const string CanadaHealthNumberSystem = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-health-card-number";
        private const string OntarioHealthNumberSystem = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-on-patient-hcn";
        private const string BritishColumbiaPhnSystem = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-bc-patient-healthcare-id";
        private const string AlbertaPhnSystem = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-ab-patient-healthcare-id";
        private const string QuebecNamSystem = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-qc-patient-healthcare-id";
        private const string SaskatchewanPhnSystem = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-sk-patient-healthcare-id";

        // Canadian profile URLs
        private const string CaCorePatientProfile = "http://hl7.org/fhir/ca/core/StructureDefinition/profile-patient";
        private const string CaCorePractitionerProfile = "http://hl7.org/fhir/ca/core/StructureDefinition/profile-practitioner";
        private const string CaCoreOrganizationProfile = "http://hl7.org/fhir/ca/core/StructureDefinition/profile-organization";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string fhirServerUrl = "http://hapi.fhir.org/baseR4";

            Console.WriteLine("FHIR .NET SDK - Canadian FHIR Profiles Tutorial");
            Console.WriteLine("===============================================");
            Console.WriteLine();
            Console.WriteLine("This tutorial demonstrates how to work with Canadian national");
            Console.WriteLine("FHIR profiles (CA Core) for healthcare interoperability in Canada.");
            Console.WriteLine();

            try
            {
                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    ReturnPreference = ReturnPreference.Representation
                };

                var fhirClient = new FhirClient(fhirServerUrl, settings);

                // Step 1: Create a CA Core compliant Patient with Ontario Health Number
                Console.WriteLine("Step 1: Creating a CA Core Patient with Ontario Health Number (OHIP)");
                Console.WriteLine("--------------------------------------------------------------------");

                var ontarioPatient = CreateOntarioPatient();
                PrintPatientSummary("Ontario Patient", ontarioPatient);
                Console.WriteLine();

                // Step 2: Create a CA Core compliant Patient with British Columbia PHN
                Console.WriteLine("Step 2: Creating a CA Core Patient with British Columbia PHN");
                Console.WriteLine("------------------------------------------------------------");

                var bcPatient = CreateBritishColumbiaPatient();
                PrintPatientSummary("BC Patient", bcPatient);
                Console.WriteLine();

                // Step 3: Create a Patient with Quebec NAM (Numero d'assurance maladie)
                Console.WriteLine("Step 3: Creating a CA Core Patient with Quebec NAM");
                Console.WriteLine("--------------------------------------------------");

                var quebecPatient = CreateQuebecPatient();
                PrintPatientSummary("Quebec Patient", quebecPatient);
                Console.WriteLine();

                // Step 4: Create a CA Core compliant Patient with Saskatchewan Health Services Number
                Console.WriteLine("Step 4: Creating a CA Core Patient with Saskatchewan HSN");
                Console.WriteLine("--------------------------------------------------------");

                var saskPatient = CreateSaskatchewanPatient();
                PrintPatientSummary("Saskatchewan Patient", saskPatient);
                Console.WriteLine();

                // Step 5: Create a CA Core compliant Patient with Alberta Personal Health Number
                Console.WriteLine("Step 5: Creating a CA Core Patient with Alberta PHN");
                Console.WriteLine("---------------------------------------------------");

                var albertaPatient = CreateAlbertaPatient();
                PrintPatientSummary("Alberta Patient", albertaPatient);
                Console.WriteLine();

                // Step 6: Create a CA Core compliant Practitioner (Canadian Physician)
                Console.WriteLine("Step 6: Creating a CA Core Practitioner (Canadian Physician)");
                Console.WriteLine("------------------------------------------------------------");

                var canadianPractitioner = CreateCanadianPractitioner();
                PrintPractitionerSummary(canadianPractitioner);
                Console.WriteLine();

                // Step 7: Create a CA Core compliant Organization (Canadian Hospital)
                Console.WriteLine("Step 7: Creating a CA Core Organization (Canadian Hospital)");
                Console.WriteLine("-----------------------------------------------------------");

                var canadianHospital = CreateCanadianOrganization();
                PrintOrganizationSummary(canadianHospital);
                Console.WriteLine();

                // Step 8: Validate resources using serialization
                Console.WriteLine("Step 8: Validating Canadian Resources (Serialization Check)");
                Console.WriteLine("-----------------------------------------------------------");

                var serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = true });

                Console.WriteLine("Validating Ontario Patient...");
                try
                {
                    string json = serializer.SerializeToString(ontarioPatient);
                    Console.WriteLine("  Result: PASSED (serializes correctly)");
                    Console.WriteLine($"  Serialized length: {json.Length} characters");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Result: FAILED - {ex.Message}");
                }

                Console.WriteLine("Validating BC Patient...");
                try
                {
                    string json = serializer.SerializeToString(bcPatient);
                    Console.WriteLine("  Result: PASSED (serializes correctly)");
                    Console.WriteLine($"  Serialized length: {json.Length} characters");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Result: FAILED - {ex.Message}");
                }

                Console.WriteLine("Validating Practitioner...");
                try
                {
                    string json = serializer.SerializeToString(canadianPractitioner);
                    Console.WriteLine("  Result: PASSED (serializes correctly)");
                    Console.WriteLine($"  Serialized length: {json.Length} characters");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Result: FAILED - {ex.Message}");
                }
                Console.WriteLine();

                // Step 9: Submit resources to FHIR server
                Console.WriteLine("Step 9: Submitting Canadian Resources to FHIR Server");
                Console.WriteLine("----------------------------------------------------");

                // Create Ontario Patient on server
                Console.WriteLine("Creating Ontario Patient on server...");
                var createdOntarioPatient = await fhirClient.CreateAsync(ontarioPatient);
                if (createdOntarioPatient != null)
                {
                    Console.WriteLine($"  Ontario Patient created with ID: {createdOntarioPatient.Id}");
                    Console.WriteLine($"  URL: {fhirServerUrl}/Patient/{createdOntarioPatient.Id}");
                }

                // Create Practitioner on server
                Console.WriteLine("Creating Canadian Practitioner on server...");
                var createdPractitioner = await fhirClient.CreateAsync(canadianPractitioner);
                if (createdPractitioner != null)
                {
                    Console.WriteLine($"  Practitioner created with ID: {createdPractitioner.Id}");
                    Console.WriteLine($"  URL: {fhirServerUrl}/Practitioner/{createdPractitioner.Id}");
                }

                // Create Organization on server
                Console.WriteLine("Creating Canadian Hospital on server...");
                var createdOrganization = await fhirClient.CreateAsync(canadianHospital);
                if (createdOrganization != null)
                {
                    Console.WriteLine($"  Organization created with ID: {createdOrganization.Id}");
                    Console.WriteLine($"  URL: {fhirServerUrl}/Organization/{createdOrganization.Id}");
                }
                Console.WriteLine();

                // Step 10: Search for patients by provincial health number
                Console.WriteLine("Step 10: Searching for Patients by Provincial Health Number");
                Console.WriteLine("-----------------------------------------------------------");

                var searchParams = new SearchParams()
                    .Where($"identifier={OntarioHealthNumberSystem}|");

                var searchResults = await fhirClient.SearchAsync<Patient>(searchParams);

                Console.WriteLine($"Found {searchResults?.Total ?? 0} patients with Ontario health numbers.");
                if (searchResults?.Entry != null)
                {
                    int count = Math.Min(searchResults.Entry.Count, 3);
                    for (int i = 0; i < count; i++)
                    {
                        if (searchResults.Entry[i].Resource is Patient p)
                        {
                            Console.WriteLine($"  - Patient ID: {p.Id}");
                        }
                    }
                }
                Console.WriteLine();

                // Step 11: Server-side validation
                Console.WriteLine("Step 11: Server-Side Validation of Canadian Patient");
                Console.WriteLine("---------------------------------------------------");

                var validateParams = new Parameters();
                validateParams.Add("resource", ontarioPatient);

                try
                {
                    var validationResult = await fhirClient.TypeOperationAsync<Patient>("validate", validateParams);

                    if (validationResult is OperationOutcome outcome)
                    {
                        Console.WriteLine("Server validation completed.");
                        PrintValidationOutcome(outcome);
                    }
                    else if (validationResult is Parameters resultParams)
                    {
                        Console.WriteLine("Server validation completed.");
                        foreach (var param in resultParams.Parameter)
                        {
                            if (param.Resource is OperationOutcome paramOutcome)
                            {
                                PrintValidationOutcome(paramOutcome);
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

                // Print Canadian FHIR Profile Best Practices
                Console.WriteLine("Canadian FHIR Profile Best Practices:");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("1. Always use the appropriate provincial health number system URI");
                Console.WriteLine("2. Use Canadian postal code format (A1A 1A1) in addresses");
                Console.WriteLine("3. Reference CA Core profile URLs in meta.profile");
                Console.WriteLine("4. Use standard Canadian province/territory codes (ON, BC, QC, etc.)");
                Console.WriteLine("5. Include both English and French names where applicable");
                Console.WriteLine("6. Follow Infoway's pan-Canadian standards for interoperability");
                Console.WriteLine("7. Validate resources against CA Core profiles before production use");
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
            Console.WriteLine("Tutorial completed.");

            if (Environment.GetEnvironmentVariable("FHIR_TUTORIAL_TEST_MODE") != "true")
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Creates a CA Core compliant Patient with Ontario Health Insurance Plan (OHIP) number.
        /// OHIP is Ontario's provincial health insurance system.
        /// </summary>
        private static Patient CreateOntarioPatient()
        {
            var patient = new Patient
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCorePatientProfile }
                },
                Identifier = new List<Identifier>
                {
                    // Ontario Health Number (OHIP) - Format: 10 digits (e.g., 1234567890)
                    new Identifier
                    {
                        System = OntarioHealthNumberSystem,
                        Value = "1234567890",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "JHN",
                                    Display = "Jurisdictional health number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Official,
                        Period = new Period { Start = "2020-01-01" }
                    },
                    // Secondary identifier (Medical Record Number)
                    new Identifier
                    {
                        System = "http://example.org/hospital/mrn",
                        Value = "MRN-ON-12345",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "MR",
                                    Display = "Medical record number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Usual
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Family = "Tremblay",
                        Given = new List<string> { "Marie", "Claire" },
                        Prefix = new List<string> { "Ms." }
                    }
                },
                Gender = AdministrativeGender.Female,
                BirthDate = "1985-03-15",
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Home,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "123 Yonge Street", "Unit 456" },
                        City = "Toronto",
                        State = "ON",  // Canadian province code
                        PostalCode = "M5B 1M4",  // Canadian postal code format
                        Country = "CA"  // ISO 3166 country code
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-416-555-0123",
                        Use = ContactPoint.ContactPointUse.Home
                    },
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Email,
                        Value = "marie.tremblay@example.ca",
                        Use = ContactPoint.ContactPointUse.Home
                    }
                },
                Communication = new List<Patient.CommunicationComponent>
                {
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "en-CA",
                                    Display = "English (Canada)"
                                }
                            }
                        },
                        Preferred = true
                    },
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "fr-CA",
                                    Display = "French (Canada)"
                                }
                            }
                        }
                    }
                }
            };

            return patient;
        }

        /// <summary>
        /// Creates a CA Core compliant Patient with British Columbia Personal Health Number (PHN).
        /// BC PHN is a 10-digit number used to identify BC residents for healthcare services.
        /// </summary>
        private static Patient CreateBritishColumbiaPatient()
        {
            var patient = new Patient
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCorePatientProfile }
                },
                Identifier = new List<Identifier>
                {
                    // BC Personal Health Number (PHN) - Format: 10 digits starting with 9
                    new Identifier
                    {
                        System = BritishColumbiaPhnSystem,
                        Value = "9876543210",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "JHN",
                                    Display = "Jurisdictional health number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Official
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Family = "Wong",
                        Given = new List<string> { "David", "Chen" }
                    }
                },
                Gender = AdministrativeGender.Male,
                BirthDate = "1978-08-22",
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Home,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "789 Granville Street" },
                        City = "Vancouver",
                        State = "BC",  // British Columbia
                        PostalCode = "V6Z 1K3",
                        Country = "CA"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-604-555-0199",
                        Use = ContactPoint.ContactPointUse.Mobile
                    }
                },
                Communication = new List<Patient.CommunicationComponent>
                {
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "en-CA",
                                    Display = "English (Canada)"
                                }
                            }
                        },
                        Preferred = true
                    },
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "zh-Hans",
                                    Display = "Chinese (Simplified)"
                                }
                            }
                        }
                    }
                }
            };

            return patient;
        }

        /// <summary>
        /// Creates a CA Core compliant Patient with Quebec NAM (Numero d'assurance maladie).
        /// Quebec health insurance number format: 4 letters + 8 digits (e.g., TREM85031512)
        /// </summary>
        private static Patient CreateQuebecPatient()
        {
            var patient = new Patient
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCorePatientProfile }
                },
                Identifier = new List<Identifier>
                {
                    // Quebec NAM - Format: AAAA DDDD DDDD (4 letters from name + birth date info)
                    new Identifier
                    {
                        System = QuebecNamSystem,
                        Value = "DUBO75010112",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "JHN",
                                    Display = "Jurisdictional health number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Official
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Family = "Dubois",
                        Given = new List<string> { "Jean-Pierre" },
                        Prefix = new List<string> { "M." }
                    }
                },
                Gender = AdministrativeGender.Male,
                BirthDate = "1975-01-01",
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Home,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "456 Rue Sainte-Catherine Ouest" },
                        City = "Montreal",
                        State = "QC",  // Quebec
                        PostalCode = "H3B 1A2",
                        Country = "CA"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-514-555-0145",
                        Use = ContactPoint.ContactPointUse.Home
                    }
                },
                Communication = new List<Patient.CommunicationComponent>
                {
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "fr-CA",
                                    Display = "French (Canada)"
                                }
                            }
                        },
                        Preferred = true
                    }
                }
            };

            return patient;
        }

        /// <summary>
        /// Creates a CA Core compliant Patient with Saskatchewan Health Services Number (HSN).
        /// Saskatchewan HSN is a 9-digit number used to identify Saskatchewan residents for healthcare services.
        /// The number is issued by eHealth Saskatchewan and is required for accessing provincial health services.
        /// </summary>
        private static Patient CreateSaskatchewanPatient()
        {
            var patient = new Patient
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCorePatientProfile }
                },
                Identifier = new List<Identifier>
                {
                    // Saskatchewan Health Services Number (HSN) - Format: 9 digits
                    new Identifier
                    {
                        System = SaskatchewanPhnSystem,
                        Value = "987654321",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "JHN",
                                    Display = "Jurisdictional health number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Official
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Family = "Anderson",
                        Given = new List<string> { "Sarah", "Elizabeth" },
                        Prefix = new List<string> { "Mrs." }
                    }
                },
                Gender = AdministrativeGender.Female,
                BirthDate = "1990-07-12",
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Home,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "456 Albert Street" },
                        City = "Regina",
                        State = "SK",  // Saskatchewan
                        PostalCode = "S4R 2N7",
                        Country = "CA"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-306-555-0187",
                        Use = ContactPoint.ContactPointUse.Home
                    },
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Email,
                        Value = "sarah.anderson@example.ca",
                        Use = ContactPoint.ContactPointUse.Home
                    }
                },
                Communication = new List<Patient.CommunicationComponent>
                {
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "en-CA",
                                    Display = "English (Canada)"
                                }
                            }
                        },
                        Preferred = true
                    }
                }
            };

            return patient;
        }

        /// <summary>
        /// Creates a CA Core compliant Patient with Alberta Personal Health Number (PHN).
        /// Alberta PHN is a 9-digit number issued by Alberta Health to residents for accessing
        /// provincial health services. The PHN is unique and remains with the person for life.
        /// </summary>
        private static Patient CreateAlbertaPatient()
        {
            var patient = new Patient
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCorePatientProfile }
                },
                Identifier = new List<Identifier>
                {
                    // Alberta Personal Health Number (PHN) - Format: 9 digits
                    new Identifier
                    {
                        System = AlbertaPhnSystem,
                        Value = "123456789",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "JHN",
                                    Display = "Jurisdictional health number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Official
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Family = "MacKenzie",
                        Given = new List<string> { "James", "Robert" },
                        Prefix = new List<string> { "Mr." }
                    }
                },
                Gender = AdministrativeGender.Male,
                BirthDate = "1982-11-28",
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Home,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "789 Stephen Avenue" },
                        City = "Calgary",
                        State = "AB",  // Alberta
                        PostalCode = "T2P 1G8",
                        Country = "CA"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-403-555-0156",
                        Use = ContactPoint.ContactPointUse.Home
                    },
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Email,
                        Value = "james.mackenzie@example.ca",
                        Use = ContactPoint.ContactPointUse.Home
                    }
                },
                Communication = new List<Patient.CommunicationComponent>
                {
                    new Patient.CommunicationComponent
                    {
                        Language = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "urn:ietf:bcp:47",
                                    Code = "en-CA",
                                    Display = "English (Canada)"
                                }
                            }
                        },
                        Preferred = true
                    }
                }
            };

            return patient;
        }

        /// <summary>
        /// Creates a CA Core compliant Practitioner (Canadian physician).
        /// Includes provincial medical license number and Canadian Medical Association identifier.
        /// </summary>
        private static Practitioner CreateCanadianPractitioner()
        {
            var practitioner = new Practitioner
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCorePractitionerProfile }
                },
                Identifier = new List<Identifier>
                {
                    // College of Physicians and Surgeons of Ontario (CPSO) license number
                    new Identifier
                    {
                        System = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-on-license-physician",
                        Value = "12345",
                        Type = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0203",
                                    Code = "MD",
                                    Display = "Medical License Number"
                                }
                            }
                        },
                        Use = Identifier.IdentifierUse.Official,
                        Period = new Period { Start = "2010-06-15" }
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Family = "Singh",
                        Given = new List<string> { "Priya" },
                        Prefix = new List<string> { "Dr." },
                        Suffix = new List<string> { "MD, FRCPC" }
                    }
                },
                Gender = AdministrativeGender.Female,
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Work,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "200 University Avenue" },
                        City = "Toronto",
                        State = "ON",
                        PostalCode = "M5G 1V2",
                        Country = "CA"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-416-555-0200",
                        Use = ContactPoint.ContactPointUse.Work
                    },
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Email,
                        Value = "dr.singh@hospital.ca",
                        Use = ContactPoint.ContactPointUse.Work
                    }
                },
                Qualification = new List<Practitioner.QualificationComponent>
                {
                    new Practitioner.QualificationComponent
                    {
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v2-0360",
                                    Code = "MD",
                                    Display = "Doctor of Medicine"
                                }
                            }
                        },
                        Issuer = new ResourceReference { Display = "University of Toronto" },
                        Period = new Period { Start = "2008-05-15" }
                    },
                    new Practitioner.QualificationComponent
                    {
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://snomed.info/sct",
                                    Code = "394802001",
                                    Display = "General medicine"
                                }
                            }
                        },
                        Issuer = new ResourceReference { Display = "Royal College of Physicians and Surgeons of Canada" },
                        Period = new Period { Start = "2010-06-15" }
                    }
                },
                Communication = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "urn:ietf:bcp:47",
                                Code = "en-CA",
                                Display = "English (Canada)"
                            }
                        }
                    },
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "urn:ietf:bcp:47",
                                Code = "hi",
                                Display = "Hindi"
                            }
                        }
                    },
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "urn:ietf:bcp:47",
                                Code = "pa",
                                Display = "Punjabi"
                            }
                        }
                    }
                }
            };

            return practitioner;
        }

        /// <summary>
        /// Creates a CA Core compliant Organization representing a Canadian hospital.
        /// </summary>
        private static Organization CreateCanadianOrganization()
        {
            var organization = new Organization
            {
                Meta = new Meta
                {
                    Profile = new List<string> { CaCoreOrganizationProfile }
                },
                Identifier = new List<Identifier>
                {
                    // Canadian Organization Identifier from Infoway
                    new Identifier
                    {
                        System = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-on-organization-id",
                        Value = "12345678",
                        Use = Identifier.IdentifierUse.Official
                    }
                },
                Name = "Toronto General Hospital",
                Alias = new List<string> { "TGH", "University Health Network - Toronto General" },
                Type = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://terminology.hl7.org/CodeSystem/organization-type",
                                Code = "prov",
                                Display = "Healthcare Provider"
                            }
                        }
                    },
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://terminology.hl7.org/CodeSystem/organization-type",
                                Code = "team",
                                Display = "Organizational Team"
                            }
                        },
                        Text = "Teaching Hospital"
                    }
                },
                Active = true,
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Work,
                        Type = Address.AddressType.Physical,
                        Line = new List<string> { "200 Elizabeth Street" },
                        City = "Toronto",
                        State = "ON",
                        PostalCode = "M5G 2C4",
                        Country = "CA"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "+1-416-340-4800",
                        Use = ContactPoint.ContactPointUse.Work
                    },
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Url,
                        Value = "https://www.uhn.ca/TorontoGeneral",
                        Use = ContactPoint.ContactPointUse.Work
                    }
                },
                Contact = new List<Organization.ContactComponent>
                {
                    new Organization.ContactComponent
                    {
                        Purpose = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/contactentity-type",
                                    Code = "ADMIN",
                                    Display = "Administrative"
                                }
                            }
                        },
                        Name = new HumanName
                        {
                            Family = "Administration",
                            Given = new List<string> { "Patient" }
                        },
                        Telecom = new List<ContactPoint>
                        {
                            new ContactPoint
                            {
                                System = ContactPoint.ContactPointSystem.Phone,
                                Value = "+1-416-340-4800",
                                Use = ContactPoint.ContactPointUse.Work
                            }
                        }
                    }
                }
            };

            return organization;
        }

        /// <summary>
        /// Helper method to print patient summary information.
        /// </summary>
        private static void PrintPatientSummary(string label, Patient patient)
        {
            Console.WriteLine($"{label}:");

            if (patient.Identifier?.Count > 0)
            {
                foreach (var identifier in patient.Identifier)
                {
                    var type = identifier.Type?.Coding?.FirstOrDefault()?.Display ?? "Unknown";
                    Console.WriteLine($"  Identifier ({type}): {identifier.Value}");
                    Console.WriteLine($"    System: {identifier.System}");
                }
            }

            if (patient.Name?.Count > 0)
            {
                var name = patient.Name[0];
                var givenNames = name.Given != null ? string.Join(" ", name.Given) : "";
                Console.WriteLine($"  Name: {name.Family}, {givenNames}");
            }

            Console.WriteLine($"  Gender: {patient.Gender}");
            Console.WriteLine($"  Birth Date: {patient.BirthDate ?? "N/A"}");

            if (patient.Address?.Count > 0)
            {
                var address = patient.Address[0];
                Console.WriteLine($"  Address: {string.Join(", ", address.Line ?? new List<string>())}");
                Console.WriteLine($"    City: {address.City}, {address.State} {address.PostalCode}");
                Console.WriteLine($"    Country: {address.Country}");
            }

            if (patient.Meta?.Profile?.Any() == true)
            {
                Console.WriteLine($"  Profile: {patient.Meta.Profile.First()}");
            }
        }

        /// <summary>
        /// Helper method to print practitioner summary information.
        /// </summary>
        private static void PrintPractitionerSummary(Practitioner practitioner)
        {
            Console.WriteLine("Canadian Practitioner:");

            if (practitioner.Identifier?.Count > 0)
            {
                var identifier = practitioner.Identifier[0];
                Console.WriteLine($"  License Number: {identifier.Value}");
                Console.WriteLine($"    System: {identifier.System}");
            }

            if (practitioner.Name?.Count > 0)
            {
                var name = practitioner.Name[0];
                var givenNames = name.Given != null ? string.Join(" ", name.Given) : "";
                var prefix = name.Prefix != null ? string.Join(" ", name.Prefix) + " " : "";
                var suffix = name.Suffix != null ? ", " + string.Join(", ", name.Suffix) : "";
                Console.WriteLine($"  Name: {prefix}{givenNames} {name.Family}{suffix}");
            }

            Console.WriteLine($"  Gender: {practitioner.Gender}");
            Console.WriteLine($"  Active: {practitioner.Active}");

            if (practitioner.Qualification?.Count > 0)
            {
                Console.WriteLine("  Qualifications:");
                foreach (var qual in practitioner.Qualification)
                {
                    var display = qual.Code?.Coding?.FirstOrDefault()?.Display;
                    if (!string.IsNullOrEmpty(display))
                    {
                        Console.WriteLine($"    - {display}");
                    }
                }
            }

            if (practitioner.Meta?.Profile?.Any() == true)
            {
                Console.WriteLine($"  Profile: {practitioner.Meta.Profile.First()}");
            }
        }

        /// <summary>
        /// Helper method to print organization summary information.
        /// </summary>
        private static void PrintOrganizationSummary(Organization organization)
        {
            Console.WriteLine("Canadian Organization:");
            Console.WriteLine($"  Name: {organization.Name}");

            if (organization.Identifier?.Count > 0)
            {
                var identifier = organization.Identifier[0];
                Console.WriteLine($"  Identifier: {identifier.Value}");
                Console.WriteLine($"    System: {identifier.System}");
            }

            if (organization.Type?.Count > 0)
            {
                var type = organization.Type[0].Coding?.FirstOrDefault()?.Display;
                Console.WriteLine($"  Type: {type}");
            }

            Console.WriteLine($"  Active: {organization.Active}");

            if (organization.Address?.Count > 0)
            {
                var address = organization.Address[0];
                Console.WriteLine($"  Address: {string.Join(", ", address.Line ?? new List<string>())}");
                Console.WriteLine($"    {address.City}, {address.State} {address.PostalCode}");
            }

            if (organization.Meta?.Profile?.Any() == true)
            {
                Console.WriteLine($"  Profile: {organization.Meta.Profile.First()}");
            }
        }

        /// <summary>
        /// Helper method to print validation outcome.
        /// </summary>
        private static void PrintValidationOutcome(OperationOutcome outcome)
        {
            foreach (var issue in outcome.Issue)
            {
                var severity = issue.Severity?.ToString() ?? "Unknown";
                var details = issue.Diagnostics ?? issue.Details?.Text ?? "No details";
                var location = issue.Expression != null && issue.Expression.Any()
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
