using NHapi.Base.Parser;
using NHapi.Base.Validation.Implementation;
using NHapi.Model.V24.Message;

namespace NHapiParserBasicMessageValidationDemo;

/// <summary>
/// Demonstrates HL7 message validation using NHAPI's validation framework.
/// Shows default validation, strict validation, and custom validation rules.
/// </summary>
/// <remarks>
/// NHAPI provides a flexible validation framework with three built-in contexts:
/// 1. NoValidation - Skips all validation (fastest but risky)
/// 2. DefaultValidation - Basic validation (field length limits)
/// 3. StrictValidation - Full data type validation
///
/// You can also create custom validation contexts to enforce business rules.
/// </remarks>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== NHAPI Message Validation Demo ===\n");

        // Demonstrate different validation scenarios
        DemonstrateParsingValidMessage();
        Console.WriteLine(new string('-', 60) + "\n");

        DemonstrateDefaultValidation();
        Console.WriteLine(new string('-', 60) + "\n");

        DemonstrateStrictValidation();
        Console.WriteLine(new string('-', 60) + "\n");

        DemonstrateCustomValidation();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    /// <summary>
    /// Demonstrates parsing a valid message with default validation.
    /// </summary>
    private static void DemonstrateParsingValidMessage()
    {
        Console.WriteLine("1. PARSING A VALID MESSAGE");
        Console.WriteLine("   Demonstrates successful parsing with default validation.\n");

        const string validMessage =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|" +
            "20110614075841||ACK|1407511|P|2.4||||||\r" +
            "MSA|AA|1407511|Success||";

        var parser = new PipeParser { ValidationContext = new DefaultValidation() };

        try
        {
            var message = (ACK)parser.Parse(validMessage);

            Console.WriteLine($"   Successfully parsed message:");
            Console.WriteLine($"   - Version: {message.Version}");
            Console.WriteLine($"   - Sending App: {message.MSH.SendingApplication.NamespaceID.Value}");
            Console.WriteLine($"   - ACK Code: {message.MSA.AcknowledgementCode.Value}");
            Console.WriteLine($"   - ACK Text: {message.MSA.TextMessage.Value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates default validation catching field length violations.
    /// </summary>
    /// <remarks>
    /// DefaultValidation enforces maximum field lengths based on HL7 data types.
    /// For example, the IS (Coded Value) data type has a maximum length of 200 characters.
    /// </remarks>
    private static void DemonstrateDefaultValidation()
    {
        Console.WriteLine("2. DEFAULT VALIDATION - Field Length Check");
        Console.WriteLine("   EVN-4 (IS data type) has max length of 200 characters.\n");

        // Create a string longer than 200 characters for EVN-4
        var tooLongValue = new string('X', 250);

        var messageWithLongField =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|" +
            "20110613083617||ADT^A01|2323232223232|P|2.4||||\r" +
            $"EVN|A01|20110613083617||{tooLongValue}|\r" +
            "PID|1||135769||MOUSE^MICKEY||19281118|M|||||||||||\r" +
            "PV1|1|O|||||||||||||||||||||||||||||||||||||||||";

        // DefaultValidation is the default - field length limits are enforced
        var parser = new PipeParser { ValidationContext = new DefaultValidation() };

        try
        {
            parser.Parse(messageWithLongField);
            Console.WriteLine("   Message parsed successfully (unexpected!)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   VALIDATION FAILED (expected):");
            Console.WriteLine($"   {ex.Message}");
            Console.WriteLine("\n   Note: EVN-4 exceeded the 200-character limit for IS data type.");
        }
    }

    /// <summary>
    /// Demonstrates strict validation catching data type constraint violations.
    /// </summary>
    /// <remarks>
    /// StrictValidation enforces data type constraints beyond just length.
    /// For example, SI (Sequence ID) must be a non-negative integer.
    /// </remarks>
    private static void DemonstrateStrictValidation()
    {
        Console.WriteLine("3. STRICT VALIDATION - Data Type Constraints");
        Console.WriteLine("   PID-1 (SI data type) must be a non-negative integer.\n");

        // PID-1 with negative value violates SI data type constraints
        var messageWithInvalidPid1 =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|" +
            "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
            "EVN|A01|20110613083617|||\r" +
            "PID|-1||135769||MOUSE^MICKEY||19281118|M|||||||||||\r" +
            "PV1|1|O|||||||||||||||||||||||||||||||||||||||||";

        // StrictValidation enforces full data type validation
        var parser = new PipeParser { ValidationContext = new StrictValidation() };

        try
        {
            parser.Parse(messageWithInvalidPid1);
            Console.WriteLine("   Message parsed successfully (unexpected!)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   VALIDATION FAILED (expected):");
            Console.WriteLine($"   {ex.Message}");
            Console.WriteLine("\n   Note: PID-1 cannot be negative for SI (Sequence ID) data type.");
        }
    }

    /// <summary>
    /// Demonstrates custom validation using a user-defined validation rule.
    /// </summary>
    /// <remarks>
    /// Custom validation allows you to enforce business rules beyond the HL7 standard.
    /// This example requires EVN-4 (Event Reason Code) to be populated,
    /// which is optional in the standard but may be required by your organization.
    /// </remarks>
    private static void DemonstrateCustomValidation()
    {
        Console.WriteLine("4. CUSTOM VALIDATION - Business Rules");
        Console.WriteLine("   Custom rule: EVN-4 (Event Reason Code) must be populated.\n");

        // Message with empty EVN-4 field
        var messageWithMissingEvn4 =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|" +
            "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
            "EVN|A01|20110613083617|||\r" +
            "PID|1||135769||MOUSE^MICKEY||19281118|M|||||||||||\r" +
            "PV1|1|O|||||||||||||||||||||||||||||||||||||||||";

        // Use our custom validation context which requires EVN-4
        var parser = new PipeParser { ValidationContext = new OurCustomMessageValidation() };

        try
        {
            parser.Parse(messageWithMissingEvn4);
            Console.WriteLine("   Message parsed successfully (unexpected!)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   CUSTOM VALIDATION FAILED (expected):");

            // The custom rule exception is often wrapped in an inner exception
            var message = ex.InnerException?.Message ?? ex.Message;
            Console.WriteLine($"   {message}");
            Console.WriteLine("\n   Note: Our custom rule requires EVN-4 to be populated.");
        }

        // Now try with EVN-4 populated
        Console.WriteLine("\n   Retrying with EVN-4 populated...");

        var messageWithEvn4 =
            "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|" +
            "20110613083617||ADT^A01|2323232223232|P|2.3||||\r" +
            "EVN|A01|20110613083617||ADMIT_REASON|\r" +
            "PID|1||135769||MOUSE^MICKEY||19281118|M|||||||||||\r" +
            "PV1|1|O|||||||||||||||||||||||||||||||||||||||||";

        try
        {
            parser.Parse(messageWithEvn4);
            Console.WriteLine("   Message parsed successfully with EVN-4 populated!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR: {ex.Message}");
        }
    }
}
