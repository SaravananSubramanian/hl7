using NHapi.Base.Util;

namespace NHapiTerserBasicOperations;

/// <summary>
/// Helper class that wraps NHAPI's Terser for convenient data access and modification.
/// Provides validation and null-safety around Terser operations.
/// </summary>
/// <remarks>
/// The Terser class is NHAPI's implementation of a path-based field accessor,
/// similar to XPath for XML. It allows you to access fields using simple
/// string expressions rather than navigating through the complex object model.
///
/// Common Terser expression patterns:
/// - "MSH-9" : Message type field
/// - "PID-5-1" : Patient family name
/// - "/.PID-5-2" : Relative path to given name
/// - "/.*ID-5" : Wildcard matching any segment ending in "ID"
/// - "/.PV1-9(1)-1" : Second repetition of consulting doctor
/// </remarks>
public class TerserHelper
{
    private readonly Terser _terser;

    /// <summary>
    /// Creates a new TerserHelper wrapping the specified Terser instance.
    /// </summary>
    /// <param name="terser">The Terser instance to wrap</param>
    /// <exception cref="ArgumentNullException">Thrown when terser is null</exception>
    public TerserHelper(Terser terser)
    {
        _terser = terser ?? throw new ArgumentNullException(
            nameof(terser),
            "Terser instance is required for data operations");
    }

    /// <summary>
    /// Retrieves data from the message using a Terser expression.
    /// </summary>
    /// <param name="terserExpression">
    /// The Terser expression specifying the field location.
    /// Examples: "MSH-9", "/.PID-5-1", "/.*ID-5-2", "/.PV1-9(0)-1"
    /// </param>
    /// <returns>The field value, or null if the field is empty</returns>
    /// <exception cref="ArgumentException">Thrown when expression is null or empty</exception>
    public string? GetData(string terserExpression)
    {
        ValidateExpression(terserExpression);
        return _terser.Get(terserExpression);
    }

    /// <summary>
    /// Sets data in the message using a Terser expression.
    /// </summary>
    /// <param name="terserExpression">
    /// The Terser expression specifying the field location.
    /// </param>
    /// <param name="value">The value to set (can be empty string, but not null)</param>
    /// <exception cref="ArgumentException">Thrown when expression is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public void SetData(string terserExpression, string value)
    {
        ValidateExpression(terserExpression);

        ArgumentNullException.ThrowIfNull(value, nameof(value));

        _terser.Set(terserExpression, value);
    }

    /// <summary>
    /// Attempts to get data from the message, returning a default value if not found.
    /// </summary>
    /// <param name="terserExpression">The Terser expression</param>
    /// <param name="defaultValue">Value to return if field is empty or not found</param>
    /// <returns>The field value or the default value</returns>
    public string GetDataOrDefault(string terserExpression, string defaultValue = "")
    {
        try
        {
            var result = GetData(terserExpression);
            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Checks if a field exists and has a non-empty value.
    /// </summary>
    /// <param name="terserExpression">The Terser expression</param>
    /// <returns>True if the field has a value, false otherwise</returns>
    public bool HasValue(string terserExpression)
    {
        try
        {
            var value = GetData(terserExpression);
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }

    private static void ValidateExpression(string terserExpression)
    {
        if (string.IsNullOrWhiteSpace(terserExpression))
        {
            throw new ArgumentException(
                "Terser expression cannot be null or empty",
                nameof(terserExpression));
        }
    }
}
