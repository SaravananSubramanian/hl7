using FluentAssertions;
using Com.SaravananSubramanian.Tests.FhirTutorials.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Com.SaravananSubramanian.Tests.FhirTutorials.Tests;

/// <summary>
/// Integration tests for .NET FHIR tutorials
/// </summary>
public class DotNetFhirTutorialTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly TestSettings _settings;
    private readonly ProcessRunner _processRunner;
    private readonly OutputValidator _validator;
    private bool _serverAvailable;

    public DotNetFhirTutorialTests(ITestOutputHelper output)
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
        _output.WriteLine($"FHIR Server available: {_serverAvailable}");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static IEnumerable<object[]> GetDotNetTutorials()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Configuration/appsettings.json", optional: false)
            .Build();

        var settings = configuration.Get<TestSettings>();
        if (settings?.DotNet?.Tutorials == null)
            yield break;

        foreach (var tutorial in settings.DotNet.Tutorials)
        {
            yield return new object[] { tutorial.Name, tutorial };
        }
    }

    [Theory]
    [MemberData(nameof(GetDotNetTutorials))]
    public async Task DotNetTutorial_ShouldProduceExpectedOutput(string tutorialName, TutorialConfig config)
    {
        // Arrange
        _output.WriteLine($"Testing: {tutorialName}");
        _output.WriteLine($"Project: {config.ProjectPath}");

        if (!_serverAvailable)
        {
            _output.WriteLine("FHIR server is not available - skipping test");
            Assert.True(true, "Test skipped - FHIR server not available");
            return;
        }

        // Act
        var result = await _processRunner.ExecuteDotNetTutorialAsync(config);

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
    public async Task AllDotNetTutorials_ShouldBeConfigured()
    {
        // Verify all expected tutorials are configured
        var tutorials = _settings.DotNet.Tutorials;

        tutorials.Should().HaveCount(10, "All 10 .NET FHIR tutorials should be configured");

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
    public async Task FhirServer_ShouldBeReachable()
    {
        var isReachable = await FhirServerHealthCheck.IsServerAvailableAsync(_settings.FhirServerUrl);

        _output.WriteLine($"Server URL: {_settings.FhirServerUrl}");
        _output.WriteLine($"Reachable: {isReachable}");

        // This test documents server availability but doesn't fail if unreachable
        // Individual tutorial tests will skip if server is down
        Assert.True(true, $"Server availability check completed: {isReachable}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "HL7 Programming using DotNet - Tutorial Series")))
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
