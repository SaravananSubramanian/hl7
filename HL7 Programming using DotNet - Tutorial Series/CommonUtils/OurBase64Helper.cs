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

        public byte[] ConvertFromBase64String(byte[] base64EncodedByteData)
        {
            if (base64EncodedByteData == null)
                throw new ArgumentNullException(nameof(base64EncodedByteData), "You must supply data for Base64 decoding operation");

            var decodedByteData = Convert.ToBase64String(base64EncodedByteData);
            return Convert.FromBase64String(decodedByteData);
        }

        public byte[] ConvertFromBase64String(string base64EncodedString)
        {
            if (string.IsNullOrEmpty(base64EncodedString))
                throw new ArgumentNullException(nameof(base64EncodedString), "You must supply data for Base64 decoding operation");

            return Convert.FromBase64String(base64EncodedString);
        }
    }
}
