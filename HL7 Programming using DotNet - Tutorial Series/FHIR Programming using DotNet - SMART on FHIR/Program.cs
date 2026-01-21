using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Com.SaravananSubramanian.Fhir.SmartOnFhir
{
    /// <summary>
    /// This tutorial demonstrates how to build a SMART on FHIR application using .NET.
    /// SMART on FHIR enables secure, interoperable access to healthcare data using OAuth2.
    ///
    /// Note: This is a simplified example. In production, you would need to:
    /// 1. Register your application with the FHIR server's authorization server
    /// 2. Implement proper OAuth2 flows (authorization code, client credentials, etc.)
    /// 3. Handle token refresh and expiration
    /// </summary>
    class Program
    {
        // Configuration - Replace these with your actual values
        private static readonly string ClientId = "your-client-id";
        private static readonly string ClientSecret = "your-client-secret";
        private static readonly string TenantId = "your-tenant-id";
        private static readonly string FhirServerUrl = "https://your-fhir-server.azurehealthcareapis.com";
        private static readonly string Scope = "https://your-fhir-server.azurehealthcareapis.com/.default";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("FHIR .NET SDK - SMART on FHIR Tutorial");
            Console.WriteLine("======================================");
            Console.WriteLine();

            // For demonstration purposes, we'll use a public test server
            // that doesn't require authentication
            await DemonstrateSmartOnFhirConcepts();

            Console.WriteLine();
            if (Environment.GetEnvironmentVariable("FHIR_TUTORIAL_TEST_MODE") != "true")
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static async System.Threading.Tasks.Task DemonstrateSmartOnFhirConcepts()
        {
            try
            {
                // Step 1: Understanding SMART on FHIR Authentication
                Console.WriteLine("Step 1: Understanding SMART on FHIR Authentication");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("SMART on FHIR uses OAuth2 for secure authentication.");
                Console.WriteLine("The typical flow is:");
                Console.WriteLine("  1. App redirects user to authorization server");
                Console.WriteLine("  2. User authenticates and grants permissions");
                Console.WriteLine("  3. App receives authorization code");
                Console.WriteLine("  4. App exchanges code for access token");
                Console.WriteLine("  5. App uses token to access FHIR resources");
                Console.WriteLine();

                // Step 2: OAuth2 Client Credentials Example
                Console.WriteLine("Step 2: OAuth2 Client Credentials Flow (Backend Apps)");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("For backend/service applications, use Client Credentials flow:");
                Console.WriteLine();

                PrintOAuth2Example();
                Console.WriteLine();

                // Step 3: Using the authenticated client
                Console.WriteLine("Step 3: Using an Authenticated FHIR Client");
                Console.WriteLine("------------------------------------------");

                // Demo with public test server (no auth required)
                string publicTestServer = "http://hapi.fhir.org/baseR4";

                var settings = new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                };

                var fhirClient = new FhirClient(publicTestServer, settings);

                // In production, you would add the bearer token:
                // fhirClient.OnBeforeRequest += (sender, e) =>
                // {
                //     e.RawRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                // };

                Console.WriteLine($"Connected to test server: {publicTestServer}");

                // Perform a search operation
                var patients = fhirClient.Search<Patient>(new SearchParams().LimitTo(3));
                Console.WriteLine($"Retrieved {patients.Entry?.Count ?? 0} patients");
                Console.WriteLine();

                // Step 4: SMART App Launch Framework
                Console.WriteLine("Step 4: SMART App Launch Framework");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("SMART on FHIR supports two launch modes:");
                Console.WriteLine();
                Console.WriteLine("1. EHR Launch:");
                Console.WriteLine("   - App is launched from within an EHR");
                Console.WriteLine("   - EHR provides launch context (patient, encounter, etc.)");
                Console.WriteLine("   - App receives launch parameter in authorization request");
                Console.WriteLine();
                Console.WriteLine("2. Standalone Launch:");
                Console.WriteLine("   - App launches independently");
                Console.WriteLine("   - User selects patient after authentication");
                Console.WriteLine("   - App uses standard OAuth2 authorization flow");
                Console.WriteLine();

                // Step 5: Scopes in SMART on FHIR
                Console.WriteLine("Step 5: SMART on FHIR Scopes");
                Console.WriteLine("----------------------------");
                Console.WriteLine("Common SMART scopes include:");
                Console.WriteLine("  - patient/*.read    - Read all patient data");
                Console.WriteLine("  - patient/*.write   - Write all patient data");
                Console.WriteLine("  - user/*.read       - Read data for logged-in user");
                Console.WriteLine("  - launch            - Request launch context");
                Console.WriteLine("  - launch/patient    - Request patient context");
                Console.WriteLine("  - openid            - OpenID Connect authentication");
                Console.WriteLine("  - fhirUser          - User identity claim");
                Console.WriteLine();

                // Step 6: Security Best Practices
                Console.WriteLine("Step 6: Security Best Practices");
                Console.WriteLine("-------------------------------");
                Console.WriteLine("1. Always use HTTPS for all communications");
                Console.WriteLine("2. Store client secrets securely (Azure Key Vault, etc.)");
                Console.WriteLine("3. Implement proper token refresh mechanisms");
                Console.WriteLine("4. Validate tokens before trusting claims");
                Console.WriteLine("5. Use PKCE for public clients (mobile/SPA apps)");
                Console.WriteLine("6. Log all access for audit compliance");
                Console.WriteLine("7. Implement proper error handling for auth failures");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void PrintOAuth2Example()
        {
            Console.WriteLine(@"// Example: Getting an access token using MSAL
var app = ConfidentialClientApplicationBuilder.Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(new Uri($""https://login.microsoftonline.com/{tenantId}""))
    .Build();

var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
var accessToken = result.AccessToken;

// Using the token with FHIR client
var fhirClient = new FhirClient(fhirServerUrl);
fhirClient.OnBeforeRequest += (sender, e) =>
{
    e.RawRequest.Headers.Add(""Authorization"", $""Bearer {accessToken}"");
};");
        }

        /// <summary>
        /// Example method showing how to get an access token for Azure FHIR Server.
        /// Uncomment and use with real credentials in production.
        /// </summary>
        static async Task<string> GetAccessTokenAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{TenantId}"))
                .Build();

            var result = await app.AcquireTokenForClient(new[] { Scope }).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
