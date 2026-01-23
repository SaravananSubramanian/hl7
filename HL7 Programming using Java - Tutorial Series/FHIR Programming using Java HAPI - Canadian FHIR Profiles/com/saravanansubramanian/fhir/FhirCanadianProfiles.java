package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.context.support.DefaultProfileValidationSupport;
import ca.uhn.fhir.rest.api.MethodOutcome;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import ca.uhn.fhir.validation.FhirValidator;
import ca.uhn.fhir.validation.ValidationResult;
import org.hl7.fhir.common.hapi.validation.support.*;
import org.hl7.fhir.common.hapi.validation.validator.FhirInstanceValidator;
import org.hl7.fhir.r4.model.*;

import java.time.LocalDate;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.Date;

/**
 * This tutorial demonstrates how to work with Canadian national FHIR profiles
 * using Java and the HAPI FHIR library.
 *
 * Canadian FHIR Profiles Overview:
 * ================================
 * Canada Health Infoway has developed the CA Core (Canadian Core) FHIR profiles
 * that provide Canadian-specific constraints and extensions on base FHIR resources.
 * These profiles ensure interoperability across Canadian healthcare systems.
 *
 * Key Canadian FHIR Profile Components:
 * -------------------------------------
 * 1. Provincial Health Numbers (PHN) - Unique identifiers issued by each province
 * 2. Canadian Postal Codes - Format validation (e.g., A1A 1A1)
 * 3. Province/Territory codes - Canadian jurisdictional codes
 * 4. Canadian address extensions - Specific to Canadian addressing standards
 *
 * Profile URLs referenced in this tutorial:
 * - CA Core Patient: http://hl7.org/fhir/ca/core/StructureDefinition/profile-patient
 * - CA Core Practitioner: http://hl7.org/fhir/ca/core/StructureDefinition/profile-practitioner
 * - CA Core Organization: http://hl7.org/fhir/ca/core/StructureDefinition/profile-organization
 */
public class FhirCanadianProfiles {

    // Canadian identifier system URLs
    private static final String CANADA_HEALTH_NUMBER_SYSTEM = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-health-card-number";
    private static final String ONTARIO_HEALTH_NUMBER_SYSTEM = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-on-patient-hcn";
    private static final String BRITISH_COLUMBIA_PHN_SYSTEM = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-bc-patient-healthcare-id";
    private static final String ALBERTA_PHN_SYSTEM = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-ab-patient-healthcare-id";
    private static final String QUEBEC_NAM_SYSTEM = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-qc-patient-healthcare-id";
    private static final String SASKATCHEWAN_PHN_SYSTEM = "https://fhir.infoway-inforoute.ca/NamingSystem/ca-sk-patient-healthcare-id";

    // Canadian profile URLs
    private static final String CA_CORE_PATIENT_PROFILE = "http://hl7.org/fhir/ca/core/StructureDefinition/profile-patient";
    private static final String CA_CORE_PRACTITIONER_PROFILE = "http://hl7.org/fhir/ca/core/StructureDefinition/profile-practitioner";
    private static final String CA_CORE_ORGANIZATION_PROFILE = "http://hl7.org/fhir/ca/core/StructureDefinition/profile-organization";

    public static void main(String[] args) {
        String fhirServerUrl = "http://hapi.fhir.org/baseR4";

        System.out.println("FHIR Java SDK - Canadian FHIR Profiles Tutorial");
        System.out.println("================================================");
        System.out.println();
        System.out.println("This tutorial demonstrates how to work with Canadian national");
        System.out.println("FHIR profiles (CA Core) for healthcare interoperability in Canada.");
        System.out.println();

        try {
            // Initialize FHIR context for R4
            FhirContext ctx = FhirContext.forR4();
            IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);

            // Step 1: Create a CA Core compliant Patient with Ontario Health Number
            System.out.println("Step 1: Creating a CA Core Patient with Ontario Health Number (OHIP)");
            System.out.println("--------------------------------------------------------------------");

            Patient ontarioPatient = createOntarioPatient();
            printPatientSummary("Ontario Patient", ontarioPatient);
            System.out.println();

            // Step 2: Create a CA Core compliant Patient with British Columbia PHN
            System.out.println("Step 2: Creating a CA Core Patient with British Columbia PHN");
            System.out.println("------------------------------------------------------------");

            Patient bcPatient = createBritishColumbiaPatient();
            printPatientSummary("BC Patient", bcPatient);
            System.out.println();

            // Step 3: Create a Patient with Quebec NAM (Numero d'assurance maladie)
            System.out.println("Step 3: Creating a CA Core Patient with Quebec NAM");
            System.out.println("--------------------------------------------------");

            Patient quebecPatient = createQuebecPatient();
            printPatientSummary("Quebec Patient", quebecPatient);
            System.out.println();

            // Step 4: Create a CA Core compliant Patient with Saskatchewan Health Services Number
            System.out.println("Step 4: Creating a CA Core Patient with Saskatchewan HSN");
            System.out.println("--------------------------------------------------------");

            Patient saskPatient = createSaskatchewanPatient();
            printPatientSummary("Saskatchewan Patient", saskPatient);
            System.out.println();

            // Step 5: Create a CA Core compliant Patient with Alberta Personal Health Number
            System.out.println("Step 5: Creating a CA Core Patient with Alberta PHN");
            System.out.println("---------------------------------------------------");

            Patient albertaPatient = createAlbertaPatient();
            printPatientSummary("Alberta Patient", albertaPatient);
            System.out.println();

            // Step 6: Create a CA Core compliant Practitioner (Canadian Physician)
            System.out.println("Step 6: Creating a CA Core Practitioner (Canadian Physician)");
            System.out.println("------------------------------------------------------------");

            Practitioner canadianPractitioner = createCanadianPractitioner();
            printPractitionerSummary(canadianPractitioner);
            System.out.println();

            // Step 7: Create a CA Core compliant Organization (Canadian Hospital)
            System.out.println("Step 7: Creating a CA Core Organization (Canadian Hospital)");
            System.out.println("-----------------------------------------------------------");

            Organization canadianHospital = createCanadianOrganization();
            printOrganizationSummary(canadianHospital);
            System.out.println();

            // Step 8: Validate resources locally
            System.out.println("Step 8: Validating Canadian Resources Locally");
            System.out.println("---------------------------------------------");

            ValidationSupportChain validationSupportChain = new ValidationSupportChain(
                    new DefaultProfileValidationSupport(ctx),
                    new InMemoryTerminologyServerValidationSupport(ctx),
                    new CommonCodeSystemsTerminologyService(ctx)
            );

            FhirValidator validator = ctx.newValidator();
            FhirInstanceValidator instanceValidator = new FhirInstanceValidator(validationSupportChain);
            validator.registerValidatorModule(instanceValidator);

            System.out.println("Validating Ontario Patient...");
            ValidationResult ontarioResult = validator.validateWithResult(ontarioPatient);
            System.out.println("  Result: " + (ontarioResult.isSuccessful() ? "PASSED" : "FAILED"));
            printValidationMessages(ontarioResult);

            System.out.println("Validating BC Patient...");
            ValidationResult bcResult = validator.validateWithResult(bcPatient);
            System.out.println("  Result: " + (bcResult.isSuccessful() ? "PASSED" : "FAILED"));
            printValidationMessages(bcResult);

            System.out.println("Validating Practitioner...");
            ValidationResult practitionerResult = validator.validateWithResult(canadianPractitioner);
            System.out.println("  Result: " + (practitionerResult.isSuccessful() ? "PASSED" : "FAILED"));
            printValidationMessages(practitionerResult);
            System.out.println();

            // Step 9: Submit resources to FHIR server
            System.out.println("Step 9: Submitting Canadian Resources to FHIR Server");
            System.out.println("----------------------------------------------------");

            // Create Ontario Patient on server
            System.out.println("Creating Ontario Patient on server...");
            MethodOutcome ontarioOutcome = client.create()
                    .resource(ontarioPatient)
                    .execute();

            if (ontarioOutcome.getId() != null) {
                String patientId = ontarioOutcome.getId().getIdPart();
                System.out.println("  Ontario Patient created with ID: " + patientId);
                System.out.println("  URL: " + fhirServerUrl + "/Patient/" + patientId);
            }

            // Create Practitioner on server
            System.out.println("Creating Canadian Practitioner on server...");
            MethodOutcome practitionerOutcome = client.create()
                    .resource(canadianPractitioner)
                    .execute();

            if (practitionerOutcome.getId() != null) {
                String practitionerId = practitionerOutcome.getId().getIdPart();
                System.out.println("  Practitioner created with ID: " + practitionerId);
                System.out.println("  URL: " + fhirServerUrl + "/Practitioner/" + practitionerId);
            }

            // Create Organization on server
            System.out.println("Creating Canadian Hospital on server...");
            MethodOutcome orgOutcome = client.create()
                    .resource(canadianHospital)
                    .execute();

            if (orgOutcome.getId() != null) {
                String orgId = orgOutcome.getId().getIdPart();
                System.out.println("  Organization created with ID: " + orgId);
                System.out.println("  URL: " + fhirServerUrl + "/Organization/" + orgId);
            }

            System.out.println();

            // Step 10: Search for patients by provincial health number
            System.out.println("Step 10: Searching for Patients by Provincial Health Number");
            System.out.println("-----------------------------------------------------------");

            Bundle searchResults = client.search()
                    .forResource(Patient.class)
                    .where(Patient.IDENTIFIER.hasSystemWithAnyCode(ONTARIO_HEALTH_NUMBER_SYSTEM))
                    .returnBundle(Bundle.class)
                    .execute();

            System.out.println("Found " + searchResults.getTotal() + " patients with Ontario health numbers.");
            if (searchResults.getEntry() != null) {
                int count = Math.min(searchResults.getEntry().size(), 3);
                for (int i = 0; i < count; i++) {
                    if (searchResults.getEntry().get(i).getResource() instanceof Patient) {
                        Patient p = (Patient) searchResults.getEntry().get(i).getResource();
                        System.out.println("  - Patient ID: " + p.getIdElement().getIdPart());
                    }
                }
            }
            System.out.println();

            // Print Canadian FHIR Profile Best Practices
            System.out.println("Canadian FHIR Profile Best Practices:");
            System.out.println("-------------------------------------");
            System.out.println("1. Always use the appropriate provincial health number system URI");
            System.out.println("2. Use Canadian postal code format (A1A 1A1) in addresses");
            System.out.println("3. Reference CA Core profile URLs in meta.profile");
            System.out.println("4. Use standard Canadian province/territory codes (ON, BC, QC, etc.)");
            System.out.println("5. Include both English and French names where applicable");
            System.out.println("6. Follow Infoway's pan-Canadian standards for interoperability");
            System.out.println("7. Validate resources against CA Core profiles before production use");

        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }

    /**
     * Creates a CA Core compliant Patient with Ontario Health Insurance Plan (OHIP) number.
     * OHIP is Ontario's provincial health insurance system.
     */
    private static Patient createOntarioPatient() {
        Patient patient = new Patient();

        // Set CA Core Patient profile
        patient.getMeta().addProfile(CA_CORE_PATIENT_PROFILE);

        // Add Ontario Health Number (OHIP) - Format: 10 digits (e.g., 1234567890)
        // The version code is 2 letters (e.g., AB)
        patient.addIdentifier()
                .setSystem(ONTARIO_HEALTH_NUMBER_SYSTEM)
                .setValue("1234567890")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("JHN")
                                .setDisplay("Jurisdictional health number")))
                .setUse(Identifier.IdentifierUse.OFFICIAL)
                .setPeriod(new Period()
                        .setStart(parseDate("2020-01-01")));

        // Add a secondary identifier (Medical Record Number)
        patient.addIdentifier()
                .setSystem("http://example.org/hospital/mrn")
                .setValue("MRN-ON-12345")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("MR")
                                .setDisplay("Medical record number")))
                .setUse(Identifier.IdentifierUse.USUAL);

        // Add patient name
        patient.addName()
                .setUse(HumanName.NameUse.OFFICIAL)
                .setFamily("Tremblay")
                .addGiven("Marie")
                .addGiven("Claire")
                .addPrefix("Ms.");

        // Set gender and birth date
        patient.setGender(Enumerations.AdministrativeGender.FEMALE);
        patient.setBirthDate(parseDate("1985-03-15"));
        patient.setActive(true);

        // Add Canadian address (Ontario)
        patient.addAddress()
                .setUse(Address.AddressUse.HOME)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("123 Yonge Street")
                .addLine("Unit 456")
                .setCity("Toronto")
                .setState("ON")  // Canadian province code
                .setPostalCode("M5B 1M4")  // Canadian postal code format
                .setCountry("CA");  // ISO 3166 country code

        // Add contact information
        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-416-555-0123")
                .setUse(ContactPoint.ContactPointUse.HOME);

        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.EMAIL)
                .setValue("marie.tremblay@example.ca")
                .setUse(ContactPoint.ContactPointUse.HOME);

        // Add preferred language (bilingual support)
        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("en-CA")
                                .setDisplay("English (Canada)")))
                .setPreferred(true);

        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("fr-CA")
                                .setDisplay("French (Canada)")));

        return patient;
    }

    /**
     * Creates a CA Core compliant Patient with British Columbia Personal Health Number (PHN).
     * BC PHN is a 10-digit number used to identify BC residents for healthcare services.
     */
    private static Patient createBritishColumbiaPatient() {
        Patient patient = new Patient();

        // Set CA Core Patient profile
        patient.getMeta().addProfile(CA_CORE_PATIENT_PROFILE);

        // Add BC Personal Health Number (PHN) - Format: 10 digits starting with 9
        patient.addIdentifier()
                .setSystem(BRITISH_COLUMBIA_PHN_SYSTEM)
                .setValue("9876543210")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("JHN")
                                .setDisplay("Jurisdictional health number")))
                .setUse(Identifier.IdentifierUse.OFFICIAL);

        // Add patient name
        patient.addName()
                .setUse(HumanName.NameUse.OFFICIAL)
                .setFamily("Wong")
                .addGiven("David")
                .addGiven("Chen");

        // Set gender and birth date
        patient.setGender(Enumerations.AdministrativeGender.MALE);
        patient.setBirthDate(parseDate("1978-08-22"));
        patient.setActive(true);

        // Add Canadian address (British Columbia)
        patient.addAddress()
                .setUse(Address.AddressUse.HOME)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("789 Granville Street")
                .setCity("Vancouver")
                .setState("BC")  // British Columbia
                .setPostalCode("V6Z 1K3")
                .setCountry("CA");

        // Add contact information
        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-604-555-0199")
                .setUse(ContactPoint.ContactPointUse.MOBILE);

        // Add preferred language
        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("en-CA")
                                .setDisplay("English (Canada)")))
                .setPreferred(true);

        // Add Chinese language capability
        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("zh-Hans")
                                .setDisplay("Chinese (Simplified)")));

        return patient;
    }

    /**
     * Creates a CA Core compliant Patient with Quebec NAM (Numero d'assurance maladie).
     * Quebec health insurance number format: 4 letters + 8 digits (e.g., TREM85031512)
     */
    private static Patient createQuebecPatient() {
        Patient patient = new Patient();

        // Set CA Core Patient profile
        patient.getMeta().addProfile(CA_CORE_PATIENT_PROFILE);

        // Add Quebec NAM - Format: AAAA DDDD DDDD (4 letters from name + birth date info)
        patient.addIdentifier()
                .setSystem(QUEBEC_NAM_SYSTEM)
                .setValue("DUBO75010112")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("JHN")
                                .setDisplay("Jurisdictional health number")))
                .setUse(Identifier.IdentifierUse.OFFICIAL);

        // Add patient name (French name)
        patient.addName()
                .setUse(HumanName.NameUse.OFFICIAL)
                .setFamily("Dubois")
                .addGiven("Jean-Pierre")
                .addPrefix("M.");

        // Set gender and birth date
        patient.setGender(Enumerations.AdministrativeGender.MALE);
        patient.setBirthDate(parseDate("1975-01-01"));
        patient.setActive(true);

        // Add Canadian address (Quebec)
        patient.addAddress()
                .setUse(Address.AddressUse.HOME)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("456 Rue Sainte-Catherine Ouest")
                .setCity("Montreal")
                .setState("QC")  // Quebec
                .setPostalCode("H3B 1A2")
                .setCountry("CA");

        // Add contact information
        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-514-555-0145")
                .setUse(ContactPoint.ContactPointUse.HOME);

        // Add preferred language (French)
        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("fr-CA")
                                .setDisplay("French (Canada)")))
                .setPreferred(true);

        return patient;
    }

    /**
     * Creates a CA Core compliant Patient with Saskatchewan Health Services Number (HSN).
     * Saskatchewan HSN is a 9-digit number used to identify Saskatchewan residents for healthcare services.
     * The number is issued by eHealth Saskatchewan and is required for accessing provincial health services.
     */
    private static Patient createSaskatchewanPatient() {
        Patient patient = new Patient();

        // Set CA Core Patient profile
        patient.getMeta().addProfile(CA_CORE_PATIENT_PROFILE);

        // Add Saskatchewan Health Services Number (HSN) - Format: 9 digits
        patient.addIdentifier()
                .setSystem(SASKATCHEWAN_PHN_SYSTEM)
                .setValue("987654321")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("JHN")
                                .setDisplay("Jurisdictional health number")))
                .setUse(Identifier.IdentifierUse.OFFICIAL);

        // Add patient name
        patient.addName()
                .setUse(HumanName.NameUse.OFFICIAL)
                .setFamily("Anderson")
                .addGiven("Sarah")
                .addGiven("Elizabeth")
                .addPrefix("Mrs.");

        // Set gender and birth date
        patient.setGender(Enumerations.AdministrativeGender.FEMALE);
        patient.setBirthDate(parseDate("1990-07-12"));
        patient.setActive(true);

        // Add Canadian address (Saskatchewan)
        patient.addAddress()
                .setUse(Address.AddressUse.HOME)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("456 Albert Street")
                .setCity("Regina")
                .setState("SK")  // Saskatchewan
                .setPostalCode("S4R 2N7")
                .setCountry("CA");

        // Add contact information
        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-306-555-0187")
                .setUse(ContactPoint.ContactPointUse.HOME);

        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.EMAIL)
                .setValue("sarah.anderson@example.ca")
                .setUse(ContactPoint.ContactPointUse.HOME);

        // Add preferred language
        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("en-CA")
                                .setDisplay("English (Canada)")))
                .setPreferred(true);

        return patient;
    }

    /**
     * Creates a CA Core compliant Patient with Alberta Personal Health Number (PHN).
     * Alberta PHN is a 9-digit number issued by Alberta Health to residents for accessing
     * provincial health services. The PHN is unique and remains with the person for life.
     */
    private static Patient createAlbertaPatient() {
        Patient patient = new Patient();

        // Set CA Core Patient profile
        patient.getMeta().addProfile(CA_CORE_PATIENT_PROFILE);

        // Add Alberta Personal Health Number (PHN) - Format: 9 digits
        patient.addIdentifier()
                .setSystem(ALBERTA_PHN_SYSTEM)
                .setValue("123456789")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("JHN")
                                .setDisplay("Jurisdictional health number")))
                .setUse(Identifier.IdentifierUse.OFFICIAL);

        // Add patient name
        patient.addName()
                .setUse(HumanName.NameUse.OFFICIAL)
                .setFamily("MacKenzie")
                .addGiven("James")
                .addGiven("Robert")
                .addPrefix("Mr.");

        // Set gender and birth date
        patient.setGender(Enumerations.AdministrativeGender.MALE);
        patient.setBirthDate(parseDate("1982-11-28"));
        patient.setActive(true);

        // Add Canadian address (Alberta)
        patient.addAddress()
                .setUse(Address.AddressUse.HOME)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("789 Stephen Avenue")
                .setCity("Calgary")
                .setState("AB")  // Alberta
                .setPostalCode("T2P 1G8")
                .setCountry("CA");

        // Add contact information
        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-403-555-0156")
                .setUse(ContactPoint.ContactPointUse.HOME);

        patient.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.EMAIL)
                .setValue("james.mackenzie@example.ca")
                .setUse(ContactPoint.ContactPointUse.HOME);

        // Add preferred language
        patient.addCommunication()
                .setLanguage(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("urn:ietf:bcp:47")
                                .setCode("en-CA")
                                .setDisplay("English (Canada)")))
                .setPreferred(true);

        return patient;
    }

    /**
     * Creates a CA Core compliant Practitioner (Canadian physician).
     * Includes provincial medical license number and Canadian Medical Association identifier.
     */
    private static Practitioner createCanadianPractitioner() {
        Practitioner practitioner = new Practitioner();

        // Set CA Core Practitioner profile
        practitioner.getMeta().addProfile(CA_CORE_PRACTITIONER_PROFILE);

        // Add College of Physicians and Surgeons of Ontario (CPSO) license number
        practitioner.addIdentifier()
                .setSystem("https://fhir.infoway-inforoute.ca/NamingSystem/ca-on-license-physician")
                .setValue("12345")
                .setType(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0203")
                                .setCode("MD")
                                .setDisplay("Medical License Number")))
                .setUse(Identifier.IdentifierUse.OFFICIAL)
                .setPeriod(new Period()
                        .setStart(parseDate("2010-06-15")));

        // Add practitioner name
        practitioner.addName()
                .setUse(HumanName.NameUse.OFFICIAL)
                .setFamily("Singh")
                .addGiven("Priya")
                .addPrefix("Dr.")
                .addSuffix("MD, FRCPC");

        // Set gender
        practitioner.setGender(Enumerations.AdministrativeGender.FEMALE);
        practitioner.setActive(true);

        // Add work address
        practitioner.addAddress()
                .setUse(Address.AddressUse.WORK)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("200 University Avenue")
                .setCity("Toronto")
                .setState("ON")
                .setPostalCode("M5G 1V2")
                .setCountry("CA");

        // Add contact information
        practitioner.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-416-555-0200")
                .setUse(ContactPoint.ContactPointUse.WORK);

        practitioner.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.EMAIL)
                .setValue("dr.singh@hospital.ca")
                .setUse(ContactPoint.ContactPointUse.WORK);

        // Add qualifications
        practitioner.addQualification()
                .setCode(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://terminology.hl7.org/CodeSystem/v2-0360")
                                .setCode("MD")
                                .setDisplay("Doctor of Medicine")))
                .setIssuer(new Reference().setDisplay("University of Toronto"))
                .setPeriod(new Period()
                        .setStart(parseDate("2008-05-15")));

        // Add specialty qualification (FRCPC - Fellow of Royal College)
        practitioner.addQualification()
                .setCode(new CodeableConcept()
                        .addCoding(new Coding()
                                .setSystem("http://snomed.info/sct")
                                .setCode("394802001")
                                .setDisplay("General medicine")))
                .setIssuer(new Reference().setDisplay("Royal College of Physicians and Surgeons of Canada"))
                .setPeriod(new Period()
                        .setStart(parseDate("2010-06-15")));

        // Add language capabilities
        practitioner.addCommunication()
                .addCoding(new Coding()
                        .setSystem("urn:ietf:bcp:47")
                        .setCode("en-CA")
                        .setDisplay("English (Canada)"));

        practitioner.addCommunication()
                .addCoding(new Coding()
                        .setSystem("urn:ietf:bcp:47")
                        .setCode("hi")
                        .setDisplay("Hindi"));

        practitioner.addCommunication()
                .addCoding(new Coding()
                        .setSystem("urn:ietf:bcp:47")
                        .setCode("pa")
                        .setDisplay("Punjabi"));

        return practitioner;
    }

    /**
     * Creates a CA Core compliant Organization representing a Canadian hospital.
     */
    private static Organization createCanadianOrganization() {
        Organization organization = new Organization();

        // Set CA Core Organization profile
        organization.getMeta().addProfile(CA_CORE_ORGANIZATION_PROFILE);

        // Add organization identifiers
        // Canadian Organization Identifier from Infoway
        organization.addIdentifier()
                .setSystem("https://fhir.infoway-inforoute.ca/NamingSystem/ca-on-organization-id")
                .setValue("12345678")
                .setUse(Identifier.IdentifierUse.OFFICIAL);

        // Add organization name
        organization.setName("Toronto General Hospital");

        // Add alias (alternative names)
        organization.addAlias("TGH");
        organization.addAlias("University Health Network - Toronto General");

        // Set organization type
        organization.addType()
                .addCoding(new Coding()
                        .setSystem("http://terminology.hl7.org/CodeSystem/organization-type")
                        .setCode("prov")
                        .setDisplay("Healthcare Provider"));

        organization.addType()
                .addCoding(new Coding()
                        .setSystem("http://terminology.hl7.org/CodeSystem/organization-type")
                        .setCode("team")
                        .setDisplay("Organizational Team"))
                .setText("Teaching Hospital");

        organization.setActive(true);

        // Add address
        organization.addAddress()
                .setUse(Address.AddressUse.WORK)
                .setType(Address.AddressType.PHYSICAL)
                .addLine("200 Elizabeth Street")
                .setCity("Toronto")
                .setState("ON")
                .setPostalCode("M5G 2C4")
                .setCountry("CA");

        // Add contact information
        organization.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-416-340-4800")
                .setUse(ContactPoint.ContactPointUse.WORK);

        organization.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.URL)
                .setValue("https://www.uhn.ca/TorontoGeneral")
                .setUse(ContactPoint.ContactPointUse.WORK);

        // Add organizational contacts
        Organization.OrganizationContactComponent contact = new Organization.OrganizationContactComponent();
        contact.setPurpose(new CodeableConcept()
                .addCoding(new Coding()
                        .setSystem("http://terminology.hl7.org/CodeSystem/contactentity-type")
                        .setCode("ADMIN")
                        .setDisplay("Administrative")));
        contact.setName(new HumanName()
                .setFamily("Administration")
                .addGiven("Patient"));
        contact.addTelecom()
                .setSystem(ContactPoint.ContactPointSystem.PHONE)
                .setValue("+1-416-340-4800")
                .setUse(ContactPoint.ContactPointUse.WORK);

        organization.addContact(contact);

        return organization;
    }

    /**
     * Helper method to print patient summary information.
     */
    private static void printPatientSummary(String label, Patient patient) {
        System.out.println(label + ":");

        if (patient.hasIdentifier()) {
            for (Identifier identifier : patient.getIdentifier()) {
                String system = identifier.getSystem();
                String value = identifier.getValue();
                String type = identifier.hasType() && identifier.getType().hasCoding()
                        ? identifier.getType().getCodingFirstRep().getDisplay()
                        : "Unknown";
                System.out.println("  Identifier (" + type + "): " + value);
                System.out.println("    System: " + system);
            }
        }

        if (patient.hasName()) {
            HumanName name = patient.getNameFirstRep();
            String givenNames = name.hasGiven()
                    ? String.join(" ", name.getGiven().stream().map(StringType::getValue).toList())
                    : "";
            System.out.println("  Name: " + name.getFamily() + ", " + givenNames);
        }

        System.out.println("  Gender: " + patient.getGender());
        System.out.println("  Birth Date: " + (patient.hasBirthDate() ? patient.getBirthDateElement().getValueAsString() : "N/A"));

        if (patient.hasAddress()) {
            Address address = patient.getAddressFirstRep();
            System.out.println("  Address: " + String.join(", ", address.getLine().stream().map(StringType::getValue).toList()));
            System.out.println("    City: " + address.getCity() + ", " + address.getState() + " " + address.getPostalCode());
            System.out.println("    Country: " + address.getCountry());
        }

        if (patient.hasMeta() && patient.getMeta().hasProfile()) {
            System.out.println("  Profile: " + patient.getMeta().getProfile().get(0).getValue());
        }
    }

    /**
     * Helper method to print practitioner summary information.
     */
    private static void printPractitionerSummary(Practitioner practitioner) {
        System.out.println("Canadian Practitioner:");

        if (practitioner.hasIdentifier()) {
            Identifier identifier = practitioner.getIdentifierFirstRep();
            System.out.println("  License Number: " + identifier.getValue());
            System.out.println("    System: " + identifier.getSystem());
        }

        if (practitioner.hasName()) {
            HumanName name = practitioner.getNameFirstRep();
            String givenNames = name.hasGiven()
                    ? String.join(" ", name.getGiven().stream().map(StringType::getValue).toList())
                    : "";
            String prefix = name.hasPrefix() ? String.join(" ", name.getPrefix().stream().map(StringType::getValue).toList()) + " " : "";
            String suffix = name.hasSuffix() ? ", " + String.join(", ", name.getSuffix().stream().map(StringType::getValue).toList()) : "";
            System.out.println("  Name: " + prefix + givenNames + " " + name.getFamily() + suffix);
        }

        System.out.println("  Gender: " + practitioner.getGender());
        System.out.println("  Active: " + practitioner.getActive());

        if (practitioner.hasQualification()) {
            System.out.println("  Qualifications:");
            for (Practitioner.PractitionerQualificationComponent qual : practitioner.getQualification()) {
                if (qual.hasCode() && qual.getCode().hasCoding()) {
                    System.out.println("    - " + qual.getCode().getCodingFirstRep().getDisplay());
                }
            }
        }

        if (practitioner.hasMeta() && practitioner.getMeta().hasProfile()) {
            System.out.println("  Profile: " + practitioner.getMeta().getProfile().get(0).getValue());
        }
    }

    /**
     * Helper method to print organization summary information.
     */
    private static void printOrganizationSummary(Organization organization) {
        System.out.println("Canadian Organization:");
        System.out.println("  Name: " + organization.getName());

        if (organization.hasIdentifier()) {
            Identifier identifier = organization.getIdentifierFirstRep();
            System.out.println("  Identifier: " + identifier.getValue());
            System.out.println("    System: " + identifier.getSystem());
        }

        if (organization.hasType()) {
            System.out.println("  Type: " + organization.getTypeFirstRep().getCodingFirstRep().getDisplay());
        }

        System.out.println("  Active: " + organization.getActive());

        if (organization.hasAddress()) {
            Address address = organization.getAddressFirstRep();
            System.out.println("  Address: " + String.join(", ", address.getLine().stream().map(StringType::getValue).toList()));
            System.out.println("    " + address.getCity() + ", " + address.getState() + " " + address.getPostalCode());
        }

        if (organization.hasMeta() && organization.getMeta().hasProfile()) {
            System.out.println("  Profile: " + organization.getMeta().getProfile().get(0).getValue());
        }
    }

    /**
     * Helper method to print validation messages.
     */
    private static void printValidationMessages(ValidationResult result) {
        if (result.getMessages().isEmpty()) {
            System.out.println("    No issues found.");
        } else {
            int infoCount = 0;
            for (var message : result.getMessages()) {
                // Only show first few non-info messages to avoid clutter
                if (message.getSeverity().toString().equals("INFORMATION")) {
                    infoCount++;
                    continue;
                }
                System.out.println("    [" + message.getSeverity() + "] " + message.getMessage());
            }
            if (infoCount > 0) {
                System.out.println("    (" + infoCount + " informational messages omitted)");
            }
        }
        System.out.println();
    }

    /**
     * Helper method to parse date string to Date object.
     */
    private static Date parseDate(String dateString) {
        LocalDate localDate = LocalDate.parse(dateString, DateTimeFormatter.ISO_LOCAL_DATE);
        return Date.from(localDate.atStartOfDay(ZoneId.systemDefault()).toInstant());
    }
}
