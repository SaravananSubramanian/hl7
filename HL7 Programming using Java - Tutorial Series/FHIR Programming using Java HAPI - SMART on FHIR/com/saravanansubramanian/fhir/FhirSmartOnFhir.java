package com.saravanansubramanian.fhir;

import ca.uhn.fhir.context.FhirContext;
import ca.uhn.fhir.rest.client.api.IGenericClient;
import ca.uhn.fhir.rest.client.interceptor.BearerTokenAuthInterceptor;
import org.hl7.fhir.r4.model.*;

/**
 * This tutorial demonstrates how to build a SMART on FHIR application using Java and HAPI FHIR.
 * SMART on FHIR enables secure, interoperable access to healthcare data using OAuth2.
 *
 * Note: This is a simplified example demonstrating the concepts. In production, you would need to:
 * 1. Register your application with the FHIR server's authorization server
 * 2. Implement proper OAuth2 flows (authorization code, client credentials, etc.)
 * 3. Handle token refresh and expiration
 */
public class FhirSmartOnFhir {

    public static void main(String[] args) {
        System.out.println("FHIR Java SDK - SMART on FHIR Tutorial");
        System.out.println("======================================");
        System.out.println();

        try {
            demonstrateSmartOnFhirConcepts();
        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }

        System.out.println();
        System.out.println("Tutorial completed.");
    }

    private static void demonstrateSmartOnFhirConcepts() {
        // Step 1: Understanding SMART on FHIR Authentication
        System.out.println("Step 1: Understanding SMART on FHIR Authentication");
        System.out.println("--------------------------------------------------");
        System.out.println("SMART on FHIR uses OAuth2 for secure authentication.");
        System.out.println("The typical flow is:");
        System.out.println("  1. App redirects user to authorization server");
        System.out.println("  2. User authenticates and grants permissions");
        System.out.println("  3. App receives authorization code");
        System.out.println("  4. App exchanges code for access token");
        System.out.println("  5. App uses token to access FHIR resources");
        System.out.println();

        // Step 2: OAuth2 Bearer Token Authentication
        System.out.println("Step 2: OAuth2 Bearer Token Authentication");
        System.out.println("------------------------------------------");
        System.out.println("Example code for using a bearer token:");
        System.out.println();
        printBearerTokenExample();
        System.out.println();

        // Step 3: Using an authenticated FHIR client (demo with public server)
        System.out.println("Step 3: Using an Authenticated FHIR Client (Demo)");
        System.out.println("------------------------------------------------");

        String publicTestServer = "http://hapi.fhir.org/baseR4";

        FhirContext ctx = FhirContext.forR4();
        IGenericClient client = ctx.newRestfulGenericClient(publicTestServer);

        // In production, you would add the bearer token interceptor:
        // String accessToken = obtainAccessToken();
        // client.registerInterceptor(new BearerTokenAuthInterceptor(accessToken));

        System.out.println("Connected to test server: " + publicTestServer);

        // Perform a search operation
        Bundle patients = client.search()
                .forResource(Patient.class)
                .count(3)
                .returnBundle(Bundle.class)
                .execute();

        int patientCount = (patients != null && patients.hasEntry()) ? patients.getEntry().size() : 0;
        System.out.println("Retrieved " + patientCount + " patients");
        System.out.println();

        // Step 4: SMART App Launch Framework
        System.out.println("Step 4: SMART App Launch Framework");
        System.out.println("-----------------------------------");
        System.out.println("SMART on FHIR supports two launch modes:");
        System.out.println();
        System.out.println("1. EHR Launch:");
        System.out.println("   - App is launched from within an EHR");
        System.out.println("   - EHR provides launch context (patient, encounter, etc.)");
        System.out.println("   - App receives launch parameter in authorization request");
        System.out.println();
        System.out.println("2. Standalone Launch:");
        System.out.println("   - App launches independently");
        System.out.println("   - User selects patient after authentication");
        System.out.println("   - App uses standard OAuth2 authorization flow");
        System.out.println();

        // Step 5: SMART on FHIR Scopes
        System.out.println("Step 5: SMART on FHIR Scopes");
        System.out.println("----------------------------");
        System.out.println("Common SMART scopes include:");
        System.out.println("  - patient/*.read    - Read all patient data");
        System.out.println("  - patient/*.write   - Write all patient data");
        System.out.println("  - user/*.read       - Read data for logged-in user");
        System.out.println("  - launch            - Request launch context");
        System.out.println("  - launch/patient    - Request patient context");
        System.out.println("  - openid            - OpenID Connect authentication");
        System.out.println("  - fhirUser          - User identity claim");
        System.out.println();

        // Step 6: Security Best Practices
        System.out.println("Step 6: Security Best Practices");
        System.out.println("-------------------------------");
        System.out.println("1. Always use HTTPS for all communications");
        System.out.println("2. Store client secrets securely (environment variables, key vaults)");
        System.out.println("3. Implement proper token refresh mechanisms");
        System.out.println("4. Validate tokens before trusting claims");
        System.out.println("5. Use PKCE for public clients (mobile/SPA apps)");
        System.out.println("6. Log all access for audit compliance");
        System.out.println("7. Implement proper error handling for auth failures");
        System.out.println();

        // Step 7: Example endpoints and test servers
        System.out.println("Step 7: SMART on FHIR Test Servers");
        System.out.println("-----------------------------------");
        System.out.println("For testing, you can use these SMART on FHIR sandboxes:");
        System.out.println("  - SMART Health IT Sandbox: https://launch.smarthealthit.org/");
        System.out.println("  - Epic's FHIR Sandbox: https://fhir.epic.com/");
        System.out.println("  - Cerner's FHIR Sandbox: https://open.cerner.com/fhir-api/");
    }

    private static void printBearerTokenExample() {
        System.out.println("// Create a FHIR context");
        System.out.println("FhirContext ctx = FhirContext.forR4();");
        System.out.println();
        System.out.println("// Create a FHIR client");
        System.out.println("IGenericClient client = ctx.newRestfulGenericClient(fhirServerUrl);");
        System.out.println();
        System.out.println("// Add bearer token authentication");
        System.out.println("String accessToken = \"your-access-token\";");
        System.out.println("BearerTokenAuthInterceptor authInterceptor = ");
        System.out.println("    new BearerTokenAuthInterceptor(accessToken);");
        System.out.println("client.registerInterceptor(authInterceptor);");
        System.out.println();
        System.out.println("// Now all requests will include the Authorization header");
        System.out.println("Patient patient = client.read()");
        System.out.println("    .resource(Patient.class)");
        System.out.println("    .withId(\"123\")");
        System.out.println("    .execute();");
    }
}
