using System;
using System.IO;

namespace CommonUtils
{
    public class OurBase64Helper
    {
        public string ConvertToBase64String(FileInfo inputFile)
        {
            if (!inputFile.Exists)
                throw new FileNotFoundException($"The specified input file {inputFile.Name} does not exist");

            return Convert.ToBase64String(File.ReadAllBytes(inputFile.FullName));
        }

        public byte[] ConvertFromBase64String(string base64EncodedString)
        {
            if (string.IsNullOrEmpty(base64EncodedString))
                throw new ArgumentNullException(nameof(base64EncodedString), "You must supply data for Base64 decoding operation");

            if (base64EncodedString.Length % 4 != 0)
                throw new BadBase64EncodingException("The BASE-64 encoded data is not in correct form (divide by 4 resulted in a remainder)");

            try
            {
                return Convert.FromBase64String(base64EncodedString);
            }
            catch (Exception )
            {
                throw new ApplicationException("Unable to decode Base-64 string supplied for operation. Please check your inputs") ;
            }
        }
    }
}
