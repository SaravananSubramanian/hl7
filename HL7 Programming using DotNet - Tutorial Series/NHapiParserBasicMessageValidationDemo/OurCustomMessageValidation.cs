using NHapi.Base.validation.impl;

namespace NHapiParserBasicMessageValidationDemo
{
    internal sealed class OurCustomMessageValidation : StrictValidation
    {
        public OurCustomMessageValidation()
        {
            var evn4MustBeSupplied = new Evn4MustBeSuppliedRule();
            MessageRuleBindings.Add(new RuleBinding("*", "*", evn4MustBeSupplied));
        }
    }
}