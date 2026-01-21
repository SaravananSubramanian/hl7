using NHapi.Base.Model;
using NHapi.Base.Util;
using NHapi.Base.Validation;

namespace Com.SaravananSubramanian.Nhapi.MessageValidation;

/// <summary>
/// Custom validation rule that requires the EVN-4 (Event Reason Code) field to be populated.
/// Demonstrates how to create custom message-level validation rules in NHAPI.
/// </summary>
/// <remarks>
/// The EVN-4 field contains the reason for the event being reported.
/// While optional in the HL7 standard, some organizations may require it
/// for audit trails or business rules compliance.
///
/// To create a custom validation rule:
/// 1. Implement IMessageRule interface
/// 2. Provide Description and SectionReference properties
/// 3. Implement the test() method to perform validation
/// 4. Return ValidationException array (empty if valid)
/// </remarks>
public class Evn4MustBeSuppliedRule : IMessageRule
{
    /// <summary>
    /// Human-readable description of what this rule validates.
    /// This is included in validation error messages.
    /// </summary>
    public virtual string Description => "EVN-4 (Event Reason Code) must be supplied";

    /// <summary>
    /// Reference to the HL7 specification section that defines this rule.
    /// Can be empty for custom business rules not defined in the standard.
    /// </summary>
    public virtual string SectionReference => "Custom Business Rule";

    /// <summary>
    /// Tests whether the message passes this validation rule.
    /// </summary>
    /// <param name="msg">The HL7 message to validate</param>
    /// <returns>
    /// An array of ValidationException objects. Empty array indicates the message is valid.
    /// </returns>
    public ValidationException[] test(IMessage msg)
    {
        // Use Terser for easy field access using location expressions
        // EVN-4 refers to the fourth field of the EVN segment
        var terser = new Terser(msg);

        try
        {
            var evn4Value = terser.Get("EVN-4");

            if (string.IsNullOrWhiteSpace(evn4Value))
            {
                return
                [
                    new ValidationException(
                        $"{Description}. The event reason code field is empty or missing.")
                ];
            }

            // Validation passed
            return [];
        }
        catch (Exception ex)
        {
            // If the segment doesn't exist or can't be accessed, treat as validation failure
            return
            [
                new ValidationException(
                    $"{Description}. Error accessing EVN segment: {ex.Message}")
            ];
        }
    }

    public ValidationException[] Test(IMessage msg)
    {
        throw new NotImplementedException();
    }
}
