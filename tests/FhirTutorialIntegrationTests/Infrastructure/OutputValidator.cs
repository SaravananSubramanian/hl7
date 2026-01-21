using System.Text.RegularExpressions;

namespace Com.SaravananSubramanian.Tests.FhirTutorials.Infrastructure;

/// <summary>
/// Validates tutorial output against expected patterns
/// </summary>
public class OutputValidator
{
    private static readonly string[] DefaultErrorPatterns =
    {
        @"Unhandled exception\.",
        @"System\.Exception",
        @"System\.InvalidOperationException.*Cannot read keys", // Expected in non-interactive mode, ignore
    };

    private static readonly string[] FatalErrorPatterns =
    {
        @"Error connecting to FHIR server",
        @"System\.Net\.Http\.HttpRequestException",
        @"The SSL connection could not be established",
        @"No such host is known",
    };

    /// <summary>
    /// Validate the tutorial execution result
    /// </summary>
    public ValidationResult Validate(TutorialExecutionResult execution, TutorialConfig config)
    {
        var result = new ValidationResult();
        var output = execution.Output ?? "";
        var combinedOutput = output + "\n" + (execution.Error ?? "");

        // Check if process failed to start
        if (execution.StartFailed)
        {
            result.IsSuccess = false;
            result.DetectedErrors.Add($"Process failed to start: {execution.StartFailureReason}");
            return result;
        }

        // Check for fatal errors (connection issues, etc.)
        foreach (var pattern in FatalErrorPatterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (regex.IsMatch(combinedOutput))
            {
                result.IsSuccess = false;
                result.DetectedErrors.Add($"Fatal error detected: {pattern}");
            }
        }

        // Check required patterns
        foreach (var pattern in config.RequiredPatterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (regex.IsMatch(output))
            {
                result.MatchedPatterns.Add(pattern);
            }
            else
            {
                result.MissingPatterns.Add(pattern);
            }
        }

        // Check for custom error patterns specific to this tutorial
        foreach (var pattern in config.ErrorPatterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regex.Matches(combinedOutput);
            foreach (Match match in matches)
            {
                result.DetectedErrors.Add(ExtractErrorContext(combinedOutput, match.Index, pattern));
            }
        }

        // Determine overall success
        // Note: We don't fail on "Cannot read keys" error as that's expected behavior
        if (result.MissingPatterns.Count > 0)
        {
            result.IsSuccess = false;
        }

        if (result.DetectedErrors.Any(e => !e.Contains("Cannot read keys")))
        {
            result.IsSuccess = false;
        }

        // Timeout is acceptable if we got the expected output (due to Console.ReadKey blocking)
        if (execution.TimedOut && result.MissingPatterns.Count == 0 && !result.DetectedErrors.Any(e => !e.Contains("Cannot read keys")))
        {
            result.IsSuccess = true;
        }

        return result;
    }

    /// <summary>
    /// Quick check if the FHIR server is reachable
    /// </summary>
    public static async Task<bool> IsFhirServerReachableAsync(string serverUrl)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await httpClient.GetAsync($"{serverUrl}/metadata");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractErrorContext(string output, int matchIndex, string pattern)
    {
        // Get surrounding context (up to 100 chars before and after)
        var start = Math.Max(0, matchIndex - 50);
        var end = Math.Min(output.Length, matchIndex + 100);
        var context = output.Substring(start, end - start).Trim();

        // Replace newlines with spaces for cleaner display
        context = Regex.Replace(context, @"\s+", " ");

        return $"Pattern '{pattern}' matched: ...{context}...";
    }
}
