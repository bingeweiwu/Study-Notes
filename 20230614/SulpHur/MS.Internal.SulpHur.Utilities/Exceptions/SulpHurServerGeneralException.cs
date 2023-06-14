using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.Utilities.Exceptions
{
    public class SulpHurServerGeneralException: Exception
    {
        public SulpHurServerGeneralException()
            : base()
        {
        }
        public SulpHurServerGeneralException(string message)
            : base(message)
        {
        }
    }
}
