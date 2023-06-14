using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.Utilities.Exceptions
{
    public class SulpHurClientUIChangedException: Exception
    {
        public SulpHurClientUIChangedException()
            : base()
        {
        }
        public SulpHurClientUIChangedException(string message)
            : base(message)
        {
        }
    }
}
