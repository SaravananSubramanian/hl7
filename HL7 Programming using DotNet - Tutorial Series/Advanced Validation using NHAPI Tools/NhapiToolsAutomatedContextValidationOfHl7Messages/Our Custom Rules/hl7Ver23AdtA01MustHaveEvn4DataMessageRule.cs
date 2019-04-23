using NHapi.Base.Util;
using NHapi.Base.validation;
using NHapiTools.Base.Validation;

namespace NhapiToolsAutomatedContextValidationOfHl7Messages.Our_Custom_Rules
{
    public class Hl7Ver23AdtA01MessageMustHaveEvn4DataRule : ISpecificMessageRule
    {
        public string[] GetVersions()
        {
            return new[] {"2.3"};
        }
        
        public string[] GetMessageTypes()
        {
            return new[] { "ADT" };
        }

        public string[] GetTriggerEvents()
        {
            return new[] { "A01" };
        }

        public ValidationException[] test(NHapi.Base.Model.IMessage msg)
        {
            var validationResults = new ValidationException[0];
            var terser = new Terser(msg);
            var value = terser.Get("EVN-4");

            if (string.IsNullOrEmpty(value))
            {
                validationResults = new[] { new ValidationException(Description) };
            }

            return validationResults;
        }

        public string Description => "EVN-4 must be supplied";

        public string SectionReference => string.Empty;
    }
}
