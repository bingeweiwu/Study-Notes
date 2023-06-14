using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Common
{
    public static class ExtensionMethods
    {
        public static bool Contains(this string[] strArr, string strTarget, bool isCaseSensitive)
        {
            if (isCaseSensitive)
            {
                foreach (string str in strArr)
                {
                    if (str.Equals(strTarget, StringComparison.Ordinal))
                        return true;
                }

                return false;
            }
            else
            {
                foreach (string str in strArr)
                {
                    if (str.Equals(strTarget, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
        }
    }
}