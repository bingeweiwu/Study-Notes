using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.Utilities.Exceptions
{
    public class SulpHurClientGeneralException: Exception
    {
        public SulpHurClientGeneralException()
            : base()
        {
        }
        public SulpHurClientGeneralException(string message)
            : base(message)
        {
        }
    }
}
