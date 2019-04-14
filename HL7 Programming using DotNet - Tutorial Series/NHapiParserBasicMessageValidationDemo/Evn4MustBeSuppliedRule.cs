using System;
using NHapi.Base.Model;
using NHapi.Base.Util;
using NHapi.Base.validation;

namespace NHapiParserBasicMessageValidationDemo
{
    public class Evn4MustBeSuppliedRule : IMessageRule
    {
        public virtual string Description => "EVN-4 must be supplied";

        public virtual string SectionReference => String.Empty;

        public ValidationException[] test(IMessage msg)
        {
            var validationResults = new ValidationException[0];
            var terser = new Terser(msg);
            var value = terser.Get("EVN-4");

            if (string.IsNullOrEmpty(value))
            {
                validationResults = new ValidationException[1] { new ValidationException(Description) };
            }

            return validationResults;
        }
    }
}