using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class Validation
    {
        public static bool NotNullOrEmpty(object str)
        {
            if (null == str) return false;

            if (!string.IsNullOrEmpty(str.ToString())) return true;
            return false;
        }
    }
}
