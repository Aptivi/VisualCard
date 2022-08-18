using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VisualCard.Exceptions
{
    public class VCardParseException : Exception
    {
        public VCardParseException() : base("General contact parsing error.")
        {
        }

        public VCardParseException(string message, string line, int linenumber, Exception innerException) : 
            base($"An error occured while parsing the VCard contact\n" +
                 $"Error: {message}\n" +
                 $"Line: {line}\n" + 
                 $"Line number: {linenumber}", innerException)
        {
        }
    }
}
