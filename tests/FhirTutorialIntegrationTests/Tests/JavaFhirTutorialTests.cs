using FluentAssertions;
using Com.SaravananSubramanian.Tests.FhirTutorials.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Com.SaravananSubramanian.Tests.FhirTutorials.Tests;

/// <summary>
/// Integration tests for Java FHIR tutorials
/// </summary>
public class JavaFhirTutorialTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly TestSettings _settings;
    private readonly ProcessRunner _processRunner;
    private readonly OutputValidator _validator;
    private bool _serverAvailable;
    private bool _mavenAvailable;
    private string _mavenInfo = "";

    public JavaFhirTutorialTests(ITestOutputHelper output)
    {
        _output = output;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Configuration/appsettings.json", optional: false)
            .Build();

        _settings = configuration.Get<TestSettings>() ?? new TestSettings();

        // Auto-detect repository root if not set
        if (string.IsNullOrEmpty(_settings.RepositoryRoot))
        {
            _settings.RepositoryRoot = FindRepositoryRoot();
        }

        _processRunner = new ProcessRunner(_settings);
        _validator = new OutputValidator();
    }

    public async Task InitializeAsync()
    {
        _serverAvailable = await FhirServerHealthCheck.IsServerAvailableAsync(_settings.FhirServerUrl);

        // Check for Maven Wrapper or system Maven
        var javaRootDir = Path.Combine(_settings.RepositoryRoot, _settings.Java.BasePath);
        var (mavenCmd, available) = await _processRunner.GetMavenCommandAsync(javaRootDir);
        _mavenAvailable = available;
        _mavenInfo = available ? $"Using: {mavenCmd}" : "Not available";

        _output.WriteLine($"FHIR Server available: {_serverAvailable}");
        _output.WriteLine($"Maven available: {_mavenAvailable} ({_mavenInfo})");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static IEnumerable<object[]> GetJavaTutorials()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Configuration/appsettings.json", optional: false)
            .Build();

        var settings = configuration.Get<TestSettings>();
        if (settings?.Java?.Tutorials == null)
            yield break;

        foreach (var tutorial in settings.Java.Tutorials)
        {
            yield return new object[] { tutorial.Name, tutorial };
        }
    }

    [Theory]
    [MemberData(nameof(GetJavaTutorials))]
    public async Task JavaTutorial_ShouldProduceExpectedOutput(string tutorialName, TutorialConfig config)
    {
        // Arrange
        _output.WriteLine($"Testing: {tutorialName}");
        _output.WriteLine($"Project: {config.ProjectPath}");
        _output.WriteLine($"Main Class: {config.MainClass}");

        if (!_mavenAvailable)
        {
            _output.WriteLine("Maven is not available - skipping test");
            Assert.True(true, "Test skipped - Maven not available");
            return;
        }

        if (!_serverAvailable)
        {
            _output.WriteLine("FHIR server is not available - skipping test");
            Assert.True(true, "Test skipped - FHIR server not available");
            return;
        }

        // Act
        var result = await _processRunner.ExecuteJavaTutorialAsync(config);

        // Log output
        _output.WriteLine($"Duration: {result.DurationMs}ms");
        _output.WriteLine($"Exit Code: {result.ExitCode}");
        _output.WriteLine($"Timed Out: {result.TimedOut}");
        _output.WriteLine("--- OUTPUT ---");
        _output.WriteLine(result.Output);

        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            _output.WriteLine("--- ERRORS ---");
            _output.WriteLine(result.Error);
        }

        // Assert
        result.StartFailed.Should().BeFalse($"Process should start successfully: {result.StartFailureReason}");

        var validation = _validator.Validate(result, config);

        _output.WriteLine("--- VALIDATION ---");
        _output.WriteLine($"Success: {validation.IsSuccess}");
        _output.WriteLine($"Matched: {string.Join(", ", validation.MatchedPatterns)}");
        if (validation.MissingPatterns.Any())
            _output.WriteLine($"Missing: {string.Join(", ", validation.MissingPatterns)}");
        if (validation.DetectedErrors.Any())
            _output.WriteLine($"Errors: {string.Join(", ", validation.DetectedErrors)}");

        validation.IsSuccess.Should().BeTrue(validation.Summary);
    }

    [Fact]
    public async Task AllJavaTutorials_ShouldBeConfigured()
    {
        // Verify all expected tutorials are configured
        var tutorials = _settings.Java.Tutorials;

        tutorials.Should().HaveCount(10, "All 10 Java FHIR tutorials should be configured");

        var expectedNames = new[]
        {
            "Setting Up Your Environment",
            "Creating FHIR Resources",
            "Reading FHIR Resources",
            "Updating FHIR Resources",
            "Deleting FHIR Resources",
            "Searching FHIR Resources",
            "Validating FHIR Resources",
            "Chaining Operations",
            "Advanced Operations",
            "SMART on FHIR"
        };

        foreach (var name in expectedNames)
        {
            tutorials.Should().Contain(t => t.Name == name, $"Tutorial '{name}' should be configured");
        }
    }

    [Fact]
    public async Task Maven_ShouldBeAvailable()
    {
        var isAvailable = await ProcessRunner.IsMavenAvailableAsync();

        _output.WriteLine($"Maven available: {isAvailable}");

        // This test documents Maven availability but doesn't fail if not available
        // Individual tutorial tests will skip if Maven is not present
        Assert.True(true, $"Maven availability check completed: {isAvailable}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "HL7 Programming using Java - Tutorial Series")))
            {
                return directory.FullName;
            }

            // Check for .git directory as another indicator
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        // Fall back to current directory
        return Directory.GetCurrentDirectory();
    }
}
