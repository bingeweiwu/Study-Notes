using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.Utilities
{
    public class CMUtility
    {
        // Standard format: 5.0.8847.1000; ignore the last minor version
        public static bool IsStandardFormat(string buildString)
        {
            string standardFormat = @"^5.0.[0-9]{4}.[0-9]{3}[0-9]+$";
            return Regex.IsMatch(buildString, standardFormat);
        }
    }
}
