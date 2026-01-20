using NHapi.Base.Parser;
using NHapi.Base.Util;

namespace NHapiTerserBasicOperations;

/// <summary>
/// Demonstrates using Terser expressions to navigate and extract data from HL7 messages.
/// Terser provides a powerful, path-like syntax for accessing fields without
/// navigating through the entire object model.
/// </summary>
/// <remarks>
/// Terser Expression Syntax:
///
/// Basic: SEGMENT-FIELD[-COMPONENT[-SUBCOMPONENT]]
/// Example: "MSH-9-1" = MSH segment, field 9, component 1
///
/// Relative paths: Start with "/." for relative navigation
/// Example: "/.PID-5-2" = Navigate to PID segment from root, field 5, component 2
///
/// Wildcards: Use "*" to match any characters, "?" for single character
/// Example: "/.*ID-5" = Any segment ending in "ID", field 5
///
/// Repetitions: Use (n) to specify field repetition (0-indexed)
/// Example: "/.PV1-9(1)-1" = PV1 segment, field 9, 2nd repetition, component 1
/// </remarks>
public class Program
{
    // Sample ORU R01 message for demonstration
    private const string SampleOruMessage =
        "MSH|^~\\&|LAB|HOSPITAL|HIS|FACILITY|20110613083617||ORU^R01|123456789|P|2.3|||AL|NE|\r" +
        "PID|1||123456^^^HOSPITAL^MR||SMITH^JOHN^A||19800101|M|||123 MAIN ST^^ANYTOWN^CA^12345||555-123-4567|||S||12345678|\r" +
        "PV1|1|I|ICU^101^A^HOSPITAL||||1234^JONES^SARAH^M|5678^WILLIAMS^ROBERT^J~9012^BROWN^JAMES^L||||||||1234567^ATTENDING^DOCTOR|||||||||||||||||||||||||||20110601120000|\r" +
        "ORC|RE|12345|67890|||||||||1234^JONES^SARAH|\r" +
        "OBR|1|12345|67890|80048^BASIC METABOLIC PANEL^CPT|||20110613080000|||||||20110613081500||1234^JONES^SARAH||||||||F|\r" +
        "OBX|1|NM|2345-7^GLUCOSE^LN||95|mg/dL|70-100|N|||F|||20110613082000|\r" +
        "OBX|2|NM|2160-0^CREATININE^LN||1.1|mg/dL|0.7-1.3|N|||F|||20110613082000|";

    public static void Main(string[] args)
    {
        Console.WriteLine("=== NHAPI Terser Basic Operations Demo ===\n");

        try
        {
            DemonstrateTerserOperations();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private static void DemonstrateTerserOperations()
    {
        // Parse the message
        Console.WriteLine("1. Parsing sample ORU R01 message...\n");
        var pipeParser = new PipeParser();
        var message = pipeParser.Parse(SampleOruMessage);

        // Create Terser instance wrapping the message
        var terser = new Terser(message);

        // Use our helper for convenient access
        var terserHelper = new TerserHelper(terser);

        Console.WriteLine("2. Extracting data using Terser expressions:\n");

        // Basic field access
        DemoExtract(terserHelper, "MSH-6", "Receiving Facility");
        DemoExtract(terserHelper, "MSH-9-1", "Message Type");
        DemoExtract(terserHelper, "MSH-9-2", "Trigger Event");

        Console.WriteLine();

        // Relative path navigation with /.
        Console.WriteLine("   Using relative paths (/.):");
        DemoExtract(terserHelper, "/.PID-5-1", "Patient Family Name");
        DemoExtract(terserHelper, "/.PID-5-2", "Patient Given Name");

        Console.WriteLine();

        // Wildcard expressions
        Console.WriteLine("   Using wildcards (* and ?):");
        DemoExtract(terserHelper, "/.*ID-5-2", "Using *ID pattern for PID-5-2");
        DemoExtract(terserHelper, "/.P?D-5-2", "Using P?D pattern for PID-5-2");

        Console.WriteLine();

        // Field repetitions (0-indexed)
        Console.WriteLine("   Accessing field repetitions:");
        DemoExtract(terserHelper, "/.PV1-9(0)-1", "First Consulting Doctor ID");
        DemoExtract(terserHelper, "/.PV1-9(1)-1", "Second Consulting Doctor ID");

        Console.WriteLine();

        // OBX segment access
        Console.WriteLine("   Accessing OBX segments:");
        DemoExtract(terserHelper, "/.RESPONSE/ORDER_OBSERVATION/OBSERVATION(0)/OBX-3-2", "First Test Name");
        DemoExtract(terserHelper, "/.RESPONSE/ORDER_OBSERVATION/OBSERVATION(0)/OBX-5", "First Test Value");
        DemoExtract(terserHelper, "/.RESPONSE/ORDER_OBSERVATION/OBSERVATION(1)/OBX-3-2", "Second Test Name");
        DemoExtract(terserHelper, "/.RESPONSE/ORDER_OBSERVATION/OBSERVATION(1)/OBX-5", "Second Test Value");

        Console.WriteLine();

        // Demonstrate setting values
        Console.WriteLine("3. Modifying data using Terser:\n");
        Console.WriteLine($"   Original PID-5-2: {terserHelper.GetData("/.PID-5-2")}");

        terserHelper.SetData("/.PID-5-2", "JOHNNY");
        Console.WriteLine($"   Modified PID-5-2: {terserHelper.GetData("/.PID-5-2")}");

        Console.WriteLine("\n4. Re-encoding modified message:\n");
        var modifiedMessage = pipeParser.Encode(message);
        var lines = modifiedMessage.Split('\r');
        foreach (var line in lines.Take(3))
        {
            Console.WriteLine($"   {line}");
        }
        Console.WriteLine("   ...");
    }

    private static void DemoExtract(TerserHelper helper, string expression, string description)
    {
        try
        {
            var value = helper.GetData(expression);
            Console.WriteLine($"   {description}");
            Console.WriteLine($"     Expression: {expression}");
            Console.WriteLine($"     Value: {value ?? "(empty)"}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   {description}");
            Console.WriteLine($"     Expression: {expression}");
            Console.WriteLine($"     Error: {ex.Message}\n");
        }
    }
}
