namespace Com.SaravananSubramanian.Tests.FhirTutorials.Infrastructure;

/// <summary>
/// Configuration for a single tutorial test
/// </summary>
public class TutorialConfig
{
    public required string Name { get; set; }
    public required string ProjectPath { get; set; }
    public string? MainClass { get; set; } // For Java tutorials
    public required string[] RequiredPatterns { get; set; }
    public string[] ErrorPatterns { get; set; } = Array.Empty<string>();
    public int TimeoutSeconds { get; set; } = 120;
}

/// <summary>
/// Root configuration loaded from appsettings.json
/// </summary>
public class TestSettings
{
    public string FhirServerUrl { get; set; } = "http://hapi.fhir.org/baseR4";
    public int DefaultTimeoutSeconds { get; set; } = 120;
    public string RepositoryRoot { get; set; } = "";
    public DotNetTutorialSettings DotNet { get; set; } = new();
    public JavaTutorialSettings Java { get; set; } = new();
}

public class DotNetTutorialSettings
{
    public string BasePath { get; set; } = "HL7 Programming using DotNet - Tutorial Series";
    public List<TutorialConfig> Tutorials { get; set; } = new();
}

public class JavaTutorialSettings
{
    public string BasePath { get; set; } = "HL7 Programming using Java - Tutorial Series";
    public List<TutorialConfig> Tutorials { get; set; } = new();
}

/// <summary>
/// Result of executing a tutorial
/// </summary>
public class TutorialExecutionResult
{
    public string TutorialName { get; set; } = "";
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
    public int ExitCode { get; set; }
    public long DurationMs { get; set; }
    public bool TimedOut { get; set; }
    public bool StartFailed { get; set; }
    public string? StartFailureReason { get; set; }
}

/// <summary>
/// Result of validating tutorial output
/// </summary>
public class ValidationResult
{
    public bool IsSuccess { get; set; } = true;
    public List<string> MatchedPatterns { get; set; } = new();
    public List<string> MissingPatterns { get; set; } = new();
    public List<string> DetectedErrors { get; set; } = new();
    public string Summary => IsSuccess
        ? $"Passed: {MatchedPatterns.Count} patterns matched"
        : $"Failed: Missing {MissingPatterns.Count} patterns, {DetectedErrors.Count} errors detected";
}
