using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.Utilities.Exceptions
{
    public class SulpHurClientUINotCompelteLoading : Exception
    {
        public SulpHurClientUINotCompelteLoading()
            : base()
        {
        }
        public SulpHurClientUINotCompelteLoading(string message)
            : base(message)
        {
        }
    }
}
