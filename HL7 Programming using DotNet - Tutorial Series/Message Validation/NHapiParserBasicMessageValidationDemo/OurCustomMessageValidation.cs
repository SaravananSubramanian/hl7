using NHapi.Base.Validation.Implementation;

namespace NHapiParserBasicMessageValidationDemo;

/// <summary>
/// Custom validation context that extends StrictValidation with additional business rules.
/// Demonstrates how to create a validation context with custom rules in NHAPI.
/// </summary>
/// <remarks>
/// NHAPI provides several built-in validation contexts:
/// - NoValidation: No validation (fastest, but risky)
/// - DefaultValidation: Basic validation (field length checks)
/// - StrictValidation: Strict data type validation
///
/// You can extend any of these to add custom rules while preserving the base validation.
///
/// RuleBinding parameters:
/// - First "*": Applies to all HL7 versions
/// - Second "*": Applies to all message types
/// - Rule instance: The validation rule to apply
/// </remarks>
internal sealed class OurCustomMessageValidation : StrictValidation
{
    public OurCustomMessageValidation()
    {
        // Add custom rule to require EVN-4 field
        // This rule applies to all versions and all message types
        var evn4MustBeSupplied = new Evn4MustBeSuppliedRule();
        MessageRuleBindings.Add(new RuleBinding("*", "*", evn4MustBeSupplied));

        // You can add more custom rules here:
        // MessageRuleBindings.Add(new RuleBinding("2.3", "ADT_A01", new SomeOtherRule()));
        // MessageRuleBindings.Add(new RuleBinding("*", "ORM_O01", new OrderValidationRule()));
    }
}
