namespace Com.SaravananSubramanian.Tests.FhirTutorials.Infrastructure;

/// <summary>
/// Checks FHIR server connectivity before running tests
/// </summary>
public static class FhirServerHealthCheck
{
    private static bool? _isServerAvailable;
    private static readonly SemaphoreSlim _checkLock = new(1, 1);

    /// <summary>
    /// Check if the FHIR server is available (cached result)
    /// </summary>
    public static async Task<bool> IsServerAvailableAsync(string serverUrl)
    {
        if (_isServerAvailable.HasValue)
            return _isServerAvailable.Value;

        await _checkLock.WaitAsync();
        try
        {
            if (_isServerAvailable.HasValue)
                return _isServerAvailable.Value;

            _isServerAvailable = await CheckServerAsync(serverUrl);
            return _isServerAvailable.Value;
        }
        finally
        {
            _checkLock.Release();
        }
    }

    private static async Task<bool> CheckServerAsync(string serverUrl)
    {
        try
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            // Try to get the capability statement
            var response = await httpClient.GetAsync($"{serverUrl}/metadata");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[HealthCheck] FHIR server is available: {serverUrl}");
                return true;
            }

            Console.WriteLine($"[HealthCheck] FHIR server returned {response.StatusCode}: {serverUrl}");
            return false;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[HealthCheck] Cannot connect to FHIR server: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[HealthCheck] FHIR server connection timed out: {serverUrl}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HealthCheck] Unexpected error checking FHIR server: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reset the cached server status (for testing purposes)
    /// </summary>
    public static void Reset()
    {
        _isServerAvailable = null;
    }
}
