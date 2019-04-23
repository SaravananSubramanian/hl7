using System;
using NHapi.Base.Util;

namespace HapiTerserBasicOperations
{
    public class OurTerserHelper
    {
        private readonly Terser _terser;

        public OurTerserHelper(Terser terser)
        {
            if (terser == null)
                throw new ArgumentNullException(nameof(terser),
                    "Terser object must be passed in for data retrieval operation");

            _terser = terser;
        }

        public string GetData(string terserExpression)
        {

            if (string.IsNullOrEmpty(terserExpression))
                throw new ArgumentNullException(nameof(terserExpression),
                    "Terser expression must be supplied for data retrieval operation");

            return _terser.Get(terserExpression);
        }

        public void SetData(string terserExpression, string value)
        {

            if (string.IsNullOrEmpty(terserExpression))
                throw new ArgumentNullException(nameof(terserExpression),
                    "Terser expression must be supplied for set operation");

            if (value == null) //we will let an empty string still go through
                throw new ArgumentNullException(nameof(value), "Value for set operation must be supplied");

            _terser.Set(terserExpression, value);
        }
    }

}
