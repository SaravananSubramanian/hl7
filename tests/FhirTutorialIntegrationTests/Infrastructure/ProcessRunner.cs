using System.Diagnostics;
using System.Text;

namespace Com.SaravananSubramanian.Tests.FhirTutorials.Infrastructure;

/// <summary>
/// Executes tutorials as subprocesses and captures their output
/// </summary>
public class ProcessRunner
{
    private readonly TestSettings _settings;

    public ProcessRunner(TestSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Execute a .NET tutorial project
    /// </summary>
    public async Task<TutorialExecutionResult> ExecuteDotNetTutorialAsync(
        TutorialConfig config,
        CancellationToken cancellationToken = default)
    {
        var projectPath = Path.Combine(_settings.RepositoryRoot, _settings.DotNet.BasePath, config.ProjectPath);

        if (!File.Exists(projectPath))
        {
            return new TutorialExecutionResult
            {
                TutorialName = config.Name,
                StartFailed = true,
                StartFailureReason = $"Project file not found: {projectPath}"
            };
        }

        var workingDir = Path.GetDirectoryName(projectPath)!;
        var timeoutMs = (config.TimeoutSeconds > 0 ? config.TimeoutSeconds : _settings.DefaultTimeoutSeconds) * 1000;

        return await ExecuteProcessAsync(
            "dotnet",
            $"run --project \"{projectPath}\" --no-build",
            workingDir,
            config.Name,
            timeoutMs,
            cancellationToken);
    }

    /// <summary>
    /// Execute a Java tutorial using Maven Wrapper or system Maven
    /// </summary>
    public async Task<TutorialExecutionResult> ExecuteJavaTutorialAsync(
        TutorialConfig config,
        CancellationToken cancellationToken = default)
    {
        var pomPath = Path.Combine(_settings.RepositoryRoot, _settings.Java.BasePath, config.ProjectPath);

        if (!File.Exists(pomPath))
        {
            return new TutorialExecutionResult
            {
                TutorialName = config.Name,
                StartFailed = true,
                StartFailureReason = $"POM file not found: {pomPath}"
            };
        }

        // Get the Java tutorials root directory (where mvnw is located)
        var javaRootDir = Path.Combine(_settings.RepositoryRoot, _settings.Java.BasePath);

        // Check if Maven Wrapper or system Maven is available
        var (mavenCmd, mavenAvailable) = await GetMavenCommandAsync(javaRootDir);

        if (!mavenAvailable)
        {
            return new TutorialExecutionResult
            {
                TutorialName = config.Name,
                StartFailed = true,
                StartFailureReason = "Neither Maven Wrapper nor system Maven is available"
            };
        }

        var workingDir = Path.GetDirectoryName(pomPath)!;
        var timeoutMs = (config.TimeoutSeconds > 0 ? config.TimeoutSeconds : _settings.DefaultTimeoutSeconds) * 1000;
        var mainClass = config.MainClass ?? throw new InvalidOperationException($"MainClass not specified for Java tutorial: {config.Name}");

        // Use exec:exec instead of exec:java to avoid classloader conflicts with Java's module system.
        // exec:exec spawns a separate JVM process, which avoids issues with classes like
        // javax.xml.namespace.QName being loaded by both the bootstrap classloader and Maven's classloader.
        return await ExecuteProcessAsync(
            mavenCmd,
            $"compile exec:exec -Dexec.executable=java -Dexec.args=\"-cp %classpath {mainClass}\" -q",
            workingDir,
            config.Name,
            timeoutMs,
            cancellationToken);
    }

    /// <summary>
    /// Check if Maven Wrapper or system Maven is available
    /// </summary>
    public static async Task<bool> IsMavenAvailableAsync()
    {
        // First check for system Maven
        if (await IsCommandAvailableAsync("mvn", "--version"))
            return true;

        return false;
    }

    /// <summary>
    /// Get the Maven command to use (wrapper or system)
    /// </summary>
    public async Task<(string command, bool available)> GetMavenCommandAsync(string javaRootDir)
    {
        // Check for Maven Wrapper first (preferred)
        var mvnwCmd = Path.Combine(javaRootDir, "mvnw.cmd");
        var mvnw = Path.Combine(javaRootDir, "mvnw");

        if (OperatingSystem.IsWindows() && File.Exists(mvnwCmd))
        {
            // Check if Java is available (required for Maven Wrapper)
            if (await IsJavaAvailableAsync())
            {
                return (mvnwCmd, true);
            }
        }
        else if (!OperatingSystem.IsWindows() && File.Exists(mvnw))
        {
            if (await IsJavaAvailableAsync())
            {
                return (mvnw, true);
            }
        }

        // Fall back to system Maven
        if (await IsCommandAvailableAsync("mvn", "--version"))
        {
            return ("mvn", true);
        }

        return ("", false);
    }

    /// <summary>
    /// Check if Java is available
    /// </summary>
    public static async Task<bool> IsJavaAvailableAsync()
    {
        return await IsCommandAvailableAsync("java", "-version");
    }

    /// <summary>
    /// Get JAVA_HOME path, detecting it if not set
    /// </summary>
    public static string? GetJavaHome()
    {
        // First check if JAVA_HOME is already set
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
        {
            return javaHome;
        }

        // Try to detect from java executable path
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "where" : "which",
                    Arguments = "java",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                // Get the first line (first java path found)
                var javaPath = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(javaPath))
                {
                    // Navigate up from bin/java to get JAVA_HOME
                    var javaDir = Path.GetDirectoryName(javaPath);
                    if (javaDir != null && javaDir.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
                    {
                        var potentialJavaHome = Path.GetDirectoryName(javaDir);
                        if (potentialJavaHome != null && Directory.Exists(potentialJavaHome))
                        {
                            return potentialJavaHome;
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore detection errors
        }

        return null;
    }

    /// <summary>
    /// Check if a command is available
    /// </summary>
    private static async Task<bool> IsCommandAvailableAsync(string command, string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<TutorialExecutionResult> ExecuteProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        string tutorialName,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var result = new TutorialExecutionResult { TutorialName = tutorialName };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // Set test mode environment variable to skip Console.ReadKey()
            process.StartInfo.Environment["FHIR_TUTORIAL_TEST_MODE"] = "true";

            // Set JAVA_HOME if not already set (needed for Maven Wrapper)
            if (fileName.Contains("mvnw") || fileName == "mvn" || fileName == "java")
            {
                var javaHome = GetJavaHome();
                if (!string.IsNullOrEmpty(javaHome))
                {
                    process.StartInfo.Environment["JAVA_HOME"] = javaHome;
                }
            }

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            var stopwatch = Stopwatch.StartNew();

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                // Timeout occurred - try to send input to unblock Console.ReadKey()
                try
                {
                    await process.StandardInput.WriteLineAsync("");
                    await process.StandardInput.FlushAsync();
                }
                catch { /* Ignore input errors */ }

                // Give it a moment to exit gracefully
                await Task.Delay(1000, CancellationToken.None);

                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch { /* Ignore kill errors */ }
                }

                result.TimedOut = true;
            }

            stopwatch.Stop();

            // Wait a bit for output buffers to flush
            await Task.Delay(100, CancellationToken.None);

            result.Output = outputBuilder.ToString();
            result.Error = errorBuilder.ToString();
            result.ExitCode = process.HasExited ? process.ExitCode : -1;
            result.DurationMs = stopwatch.ElapsedMilliseconds;

            return result;
        }
        catch (Exception ex)
        {
            result.StartFailed = true;
            result.StartFailureReason = ex.Message;
            return result;
        }
    }
}
